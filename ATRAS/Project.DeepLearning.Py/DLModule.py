import numpy as np
import tensorflow as tf
import os
import sys
import json

from tensorflow.python.ops.logging_ops import image_summary
from tensorflow.python.ops.map_fn import _result_flat_signature_to_batchable_tensor_spec

from tensorflow import keras
from tensorflow.keras.callbacks import ModelCheckpoint, EarlyStopping, CSVLogger

import matplotlib.pyplot as plt
from sklearn.model_selection import train_test_split
import traceback
import gc


os.environ["TF_GPU_ALLOCATOR"] = "cuda_malloc_async"

class StdoutRedirector:
    def __init__(self, write_callback):
        self.write_callback = write_callback

    def write(self, message):
        if message.strip():  # 공백 제외
            self.write_callback(message)

    def flush(self):
        pass

def redirect_stdout_to(callback):
    sys.stdout = StdoutRedirector(callback)
    sys.stderr = StdoutRedirector(callback)

class Calculator(object):
    def add123(self, a, b):
        return a + b

    def multiply123(self, a, b):
        return a * b

    def process_bytes(self, data):
        if isinstance(data, bytes):
            print(f"Python received bytes: {data}")

            for byte in data:
                print(f"Byte Value : {byte}")

            return len(data)
        else:
            print("Error : Received data is not a bytes object.")
            return -1

    def process_bytes2(self, data_list):
        if isinstance(data_list, list):
            print(f"Received list with {len(data_list)} items")

            for idx, item in enumerate(data_list):
                if isinstance(item, bytes):
                    print(f"[{idx}] Byte length: {len(item)} | First byte: {item[0] if item else 'empty'}")
                else:
                    print(f"[{idx}] Error: Item is not bytes")
            return len(data_list)
        else:
            print("Error: Expected a list of bytes objects.")
            return -1









gpus = tf.config.experimental.list_physical_devices('GPU')
if gpus:
    try:
        for gpu in gpus:
            tf.config.experimental.set_memory_growth(gpu, True)
        logical_gpus = tf.config.experimental.list_logical_devices('GPU')
        print(f"{len(gpus)} Physical GPUs, {len(logical_gpus)} Logical GPUs")
    except RuntimeError as e:
        print(e)
else:
    print("GPU가 감지되지 않았습니다.")


class TISClassificator(object):
    module_file_path =""
    train_ds = None
    val_ds = None


    train_scaled = None
    train_target = None

    test_scaled = None
    test_target = None

    val_scaled = None
    val_target = None

    model = None

    img_width = 0
    img_height = 0


    def __init__(self, name):
        self.name = name
        self.log_callback = None  # C#에서 넘겨줄 함수


    def SetLogCallback(self, callback):
        self.log_callback = callback

    def log(self, message):
        if self.log_callback:
            self.log_callback(message)
        else:
            print(message)

    def GetName(self):
        return self.name

    def print_device_info(self):
        import tensorflow as tf
        physical_gpus = tf.config.list_physical_devices('GPU')
        logical_gpus = tf.config.list_logical_devices('GPU')

        if physical_gpus:
            print("[Python Log] : ✅ Physical GPUs Detected:")
            for gpu in physical_gpus:
                print(f"  - {gpu}")

            print("[Python Log] : ✅ Logical GPUs Available:")
            for lgpu in logical_gpus:
                print(f"  - {lgpu}")
        else:
            print("[Python Log] : ❌ No GPU detected (running on CPU)")



    def Predict(self, data, use_gpu=True):
        if self.model is None:
            print("Model is not loaded.")
            return None

        expected_size = self.img_height * self.img_width
        if len(data) != expected_size:
            print(f"Error: Expected data of size {expected_size}, but got {len(data)}")
            return None

        try:
            data = np.frombuffer(data, dtype=np.uint8)
            print(f"Raw byte sample: {data[:10]}")

            data = data.reshape(1, self.img_height, self.img_width, 1).astype(np.float32) / 255.0
            print(f"Preprocessed shape: {data.shape}")

            device_name = "/GPU:0" if use_gpu and tf.config.list_physical_devices('GPU') else "/CPU:0"
            print(f"🖥️ Predicting on device: {device_name}")

            with tf.device(device_name):
                prediction = self.model.predict(data, verbose=0)

            pred_list = []

            pred_class = int(np.argmax(prediction))
            pred_prob = float(np.max(prediction))

            pred_list.append(pred_class)
            pred_list.append(pred_prob)
            pred_list.append(prediction)

            print(f"Predicted class: {pred_class} (confidence: {pred_prob:.4f})")
            return pred_list

        except Exception as e:
            return (f"Prediction error: {e}")

    def MultiPredict(self, data_list, use_gpu=True):
        """
        data_list : [bytes, bytes, ...] 또는 np.ndarray(batch, H, W, 1)
        여러 장의 이미지를 한 번에 예측
        """
        if self.model is None:
            print("Model is not loaded.")
            return None

        expected_size = self.img_height * self.img_width

        try:
            # -----------------------------
            # 1. 입력 데이터 처리
            # -----------------------------
            if isinstance(data_list, (list, tuple)):  
                # 리스트/튜플 (bytes 배열) → numpy 변환
                processed_data = []
                for i, data in enumerate(data_list):
                    if not isinstance(data, (bytes, bytearray, np.ndarray)):
                        raise ValueError(f"Item {i} is not bytes or ndarray")

                    if isinstance(data, (bytes, bytearray)):
                        arr = np.frombuffer(data, dtype=np.uint8)
                        if arr.size != expected_size:
                            raise ValueError(f"Item {i} size mismatch: expected {expected_size}, got {arr.size}")
                        arr = arr.reshape(self.img_height, self.img_width, 1)
                    else:
                        arr = data.reshape(self.img_height, self.img_width, 1)

                    processed_data.append(arr)

                data = np.array(processed_data, dtype=np.float32) / 255.0

            elif isinstance(data_list, np.ndarray):  
                # 이미 numpy (batch, H, W, 1) 인 경우
                if len(data_list.shape) == 3:  
                    # 단일 이미지라면 배치 차원 추가
                    data = np.expand_dims(data_list, axis=0).astype(np.float32) / 255.0
                else:
                    data = data_list.astype(np.float32) / 255.0
            else:
                raise ValueError("Input must be list/tuple of bytes or numpy array")

            print(f"✅ Preprocessed batch shape: {data.shape}")  # (N, H, W, 1)

            # -----------------------------
            # 2. 장치 설정
            # -----------------------------
            device_name = "/GPU:0" if use_gpu and tf.config.list_physical_devices('GPU') else "/CPU:0"
            print(f"🖥️ Predicting on device: {device_name}")

            # -----------------------------
            # 3. 예측 수행
            # -----------------------------
            with tf.device(device_name):
                predictions = self.model.predict(data, verbose=0)

            # -----------------------------
            # 4. 결과 정리
            # -----------------------------
            results = []
            for i, prediction in enumerate(predictions):
                pred_class = int(np.argmax(prediction))
                pred_prob = float(np.max(prediction))
                results.append({
                    "index": i,
                    "class": pred_class,
                    "confidence": pred_prob,
                    "raw": prediction.tolist()
                })
                print(f"[{i}] Predicted class: {pred_class} (confidence: {pred_prob:.4f})")

            return results

        except Exception as e:
            return (f"Prediction error: {e}")


    def training(self, epochs = 10, batch_size = 32, learning_rate = 0.001, patience=5, model_save_path="best_model.keras", log_path="training_log.csv", new_compile=True, use_gpu=True):


        target_unique = np.unique(self.train_target, return_counts=True)
        unique_count = len(target_unique[0])

        print(f'class unique count : {unique_count}')
        
        if new_compile:

            tf.keras.backend.clear_session()
            gc.collect()

            # 장치 설정
            device_name = "/GPU:0" if use_gpu and tf.config.list_physical_devices('GPU') else "/CPU:0"
            print(f"🖥️ Using device: {device_name}")

            with tf.device(device_name):
                model = tf.keras.Sequential([
                    # tf.keras.layers.Rescaling(1./255, input_shape=(self.img_height, self.img_width, 1)),            
                    tf.keras.layers.Conv2D(32, (3,3), activation='relu', input_shape=(self.img_height, self.img_width, 1)),
                    tf.keras.layers.MaxPooling2D(2,2),
                    tf.keras.layers.Conv2D(64, (3,3), activation='relu'),
                    tf.keras.layers.MaxPooling2D(2,2),
                    tf.keras.layers.Flatten(),
                    tf.keras.layers.Dense(128, activation='relu'),
                    # tf.keras.layers.Dropout(0.3),
                    tf.keras.layers.Dense(unique_count, activation='softmax')
                ])
        
                model.compile(optimizer='adam',
                            loss='sparse_categorical_crossentropy',
                            metrics=['accuracy'])
                self.model = model


        early_stopping = EarlyStopping(
                monitor='val_accuracy',
                patience=patience,
                restore_best_weights=True,
                verbose=2)

        callbacks = [
            ModelCheckpoint(
                filepath=model_save_path,
                save_best_only=True,
                monitor='val_accuracy',
                mode='max',
                verbose=2
            ),
            early_stopping,
            CSVLogger(log_path)
        ]


        #gpus = tf.config.list_physical_devices('GPU')
        #if gpus:
        #    print("✅ GPU training:", gpus[0].name)
        #else:
        #    print("⚠️ GPU not training. CPU training...")
           
        with tf.device(device_name):
            history = self.model.fit(
                self.train_scaled,
                self.train_target,
                validation_data=(self.val_scaled, self.val_target),
                epochs=epochs,
                batch_size=batch_size,
                callbacks=callbacks,
                verbose=2
            )

        stopped_epoch = early_stopping.stopped_epoch

        # Best epoch = early stopped - patience (대부분은 여기에 해당)
        # best_epoch = stopped_epoch - early_stopping.patience

        best_epoch = max(0, stopped_epoch - early_stopping.patience)

        # Accuracy / Loss at best epoch
        acc_at_best = history.history['accuracy'][best_epoch]
        val_acc_at_best = history.history['val_accuracy'][best_epoch]
        loss_at_best = history.history['loss'][best_epoch]
        val_loss_at_best = history.history['val_loss'][best_epoch]

        return json.dumps({
                            'accuracy': history.history['accuracy'],
                            'val_accuracy': history.history['val_accuracy'],
                            'loss': history.history['loss'],
                            'val_loss': history.history['val_loss'],
                            'stopped_epoch': stopped_epoch,
                            'best_epoch': best_epoch,
                            'acc_at_best': float(acc_at_best),
                            'val_acc_at_best': float(val_acc_at_best),
                            'loss_at_best': float(loss_at_best),
                            'val_loss_at_best': float(val_loss_at_best)
                        })


    def evaluate(self, use_gpu=True):
        if self.model is None:            
            return 0.0

        device_name = "/GPU:0" if use_gpu and tf.config.list_physical_devices('GPU') else "/CPU:0"
        print(f"🖥️ Evaluating on device: {device_name}")

        with tf.device(device_name):
            loss, acc = self.model.evaluate(self.test_scaled, self.test_target, verbose=2)
            print(f"✅ Test accuracy: {acc:.4f}, loss: {loss:.4f}")
            return acc

    def check_gpu(self):
        gpus = tf.config.list_physical_devices('GPU')
        if gpus:
            return True
        return False
        
    def build_module(self, width, height):

        self.img_width = width
        self.img_height = height

        model = tf.keras.Sequential([
            # tf.keras.layers.Rescaling(1./255, input_shape=(self.img_height, self.img_width, 1)),            
            tf.keras.layers.Conv2D(32, (3,3), activation='relu', input_shape=(self.img_height, self.img_width, 1)),
            tf.keras.layers.MaxPooling2D(2,2),
            tf.keras.layers.Conv2D(64, (3,3), activation='relu'),
            tf.keras.layers.MaxPooling2D(2,2),
            tf.keras.layers.Flatten(),
            tf.keras.layers.Dense(128, activation='relu'),
            tf.keras.layers.Dense(unique_count, activation='softmax')
        ])
        
        model.compile(optimizer='adam',
                    loss='sparse_categorical_crossentropy',
                    metrics=['accuracy'])

        return model
    
    def save_model(self, path='model.keras'):
        if self.model:
            self.model.save(path)
            return True

        return False

    def load_model(self, width, height, path='model.keras'):
        
        try:
            self.img_width = width
            self.img_height = height

            print(f"width : {width}, height : {height}, path : {path}")

            self.model = keras.models.load_model(path)
            if self.model:

                self.model.compile(optimizer='adam',
                loss='sparse_categorical_crossentropy',
                metrics=['accuracy'])
                return True

        except Exception as e:
            print(f"load_model : {e}")

        return False

    ######################################################################

    def LoadDataSet(self, width, height, batch_size, epochs, path):

        image_size = (width, height)
        train_ds = tf.keras.preprocessing.image_dataset_from_directory(
                path,
                validation_split=0.2,
                subset="training",
                seed=123,
                image_size=image_size,
                batch_size=batch_size
            )

        val_ds = tf.keras.preprocessing.image_dataset_from_directory(
                path,
                validation_split=0.2,
                subset="validation",
                seed=123,
                image_size=image_size,
                batch_size=batch_size
            )

        return train_ds.class_names

    
    def SetDataSet(self, width, height, input_data, target_data):

        return_list = []

        self.img_width = width
        self.img_height = height
        
        try:
            
            
            train_scaled = np.array([np.frombuffer(img_bytes, dtype=np.uint8).reshape(height, width, 1) for img_bytes in input_data])
            target_data = np.array(target_data, dtype=np.int32)
            target_data = target_data.reshape(-1)

            train_scaled = train_scaled / 255.0
            
            return_string = ""


            if isinstance(train_scaled, np.ndarray):
                return_string = "train_scaled is NumPy"
            else:
                return_string = "train_scaled is not NumPy."
                
            print(train_scaled.shape)
                                 

            train_scaled, val_scaled, train_target, val_target = train_test_split(train_scaled, target_data, test_size=0.2, random_state=42)

            train_scaled, test_scaled, train_target, test_target = train_test_split(train_scaled, train_target, test_size=0.2, random_state=42)
            
            self.train_scaled = train_scaled
            self.train_target = train_target
            self.val_scaled = val_scaled
            self.val_target = val_target
            self.test_scaled = test_scaled
            self.test_target = test_target
            
            return_list.append(self.train_scaled.shape)
            return_list.append(self.val_scaled.shape)
            return_list.append(self.test_scaled.shape)            


            return_list.append(self.train_target.shape)
            return_list.append(self.val_target.shape)
            return_list.append(self.test_target.shape)        
            
        except Exception as e:
            print(f"Error : {e}")
            

        return return_list          
        
    def SetDataSetFromBytes(self, width, height, input_data, target_data):
        return_list = []

        self.img_width = width
        self.img_height = height
        
        input_np = np.array([np.frombuffer(img_bytes, dtype=np.uint8).reshape(height, width, 1) for img_bytes in input_data])
                
        return input_np.shape

from PIL import Image
import io

def load_images_as_bytes(folder_path, width, height, allowed_extensions={'.jpg', '.jpeg', '.png', '.bmp', '.gif'}):
    input_data = []
    target_data = []

    # 하위 폴더 기준으로 클래스 자동 분류
    class_names = sorted([d for d in os.listdir(folder_path) if os.path.isdir(os.path.join(folder_path, d))])
    class_to_idx = {cls_name: idx for idx, cls_name in enumerate(class_names)}

    for class_name in class_names:
        class_folder = os.path.join(folder_path, class_name)
        for filename in os.listdir(class_folder):
            ext = os.path.splitext(filename)[1].lower()
            if ext in allowed_extensions:
                image_path = os.path.join(class_folder, filename)
                try:
                    with Image.open(image_path) as img:
                        img = img.convert('L')  # 흑백 (grayscale)
                        img = img.resize((width, height))  # 리사이즈
                        img_array = np.array(img, dtype=np.uint8).reshape(height, width, 1)
                        input_data.append(img_array)
                        target_data.append(class_to_idx[class_name])
                except Exception as e:
                    print(f"Error loading image {image_path}: {e}")

    # return np.array(input_data), np.array(target_data)
    return np.array(input_data, dtype=np.uint8), np.array(target_data, dtype=np.int32)
    
    
## 메인 함수
#def main():
#    # 스레드 리스트 생성

#    file_path = "Y:/dataset/class1/0.bmp"

#    input_d, target_d = load_images_as_bytes('D:\\2_Projects\\DeepLearning\\DataSet\\trans\\dataset', 200, 280)
    
    
#    #print(input_d.shape)
    
    
#    model_name = "ImageClassifier"
#    classifier = TISClassificator(model_name)
#    dataset_info = classifier.SetDataSet(200, 280, input_d, target_d)

#    classifier.training(epochs=20, batch_size=16, learning_rate=0.001, model_save_path="best_model.h5", log_path="training_log.csv")


#    # classifier.evaluate()
#    print(classifier.evaluate())
    
    
#if __name__ == "__main__":
#    main()