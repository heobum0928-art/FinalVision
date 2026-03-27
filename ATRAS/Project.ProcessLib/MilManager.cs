using MathNet.Numerics.LinearAlgebra;
using Matrox.MatroxImagingLibrary;
using OpenCvSharp;
using Project.BaseLib.DataStructures;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Project.ProcessLib
{   
    public class MilManager : Singleton<MilManager>, IDisposable
    {
        #region fields
        protected MIL_ID MilApplication;
        protected MIL_ID MilSystem;
        protected MIL_ID MilDisplay;

        protected Dictionary<int, MIL_ID> MilSrcImages;

        // Model Finder
        public const int ALG_MODEL_MAX_CNT = 255;
        public const int MODEL_MAX_OCCURRENCES = 1024;

        protected Dictionary<int, Dictionary<int, MIL_ID>> MilModels;

        // Calibration
        protected Dictionary<int, Dictionary<int, MIL_ID>> MilGridCalibration;

        #endregion

        #region propertise

        #endregion

        #region methods
        public bool SetMILSrcImage(int algo_id, MIL_ID MilImage)
        {
            if (MilImage == MIL.M_NULL)
                return false;

            MIL_ID MilSrcImage = GetSrcImage(algo_id);

            if(MilSrcImage != MIL.M_NULL)
            {
                MIL.MbufFree(MilSrcImage);
                MilSrcImage = MIL.M_NULL;
            }

            MilSrcImages[algo_id] = MilImage;

            

            return true;
        }

        public bool SetSrcImage(int algo_id, int width, int height, byte [] data)
        {
            MIL_ID MilImage = MIL.M_NULL;
            MIL.MbufAlloc2d(MilSystem,
                width, height,
                8L + MIL.M_UNSIGNED,
                MIL.M_IMAGE + MIL.M_PROC, ref MilImage);

            MIL.MbufPut2d(MilImage, 0, 0, width, height, data);

            return SetMILSrcImage(algo_id, MilImage);
        }

        public bool SetCvScrImage(int algo_id, Mat cv_image)
        {
            int SizeX = cv_image.Width;
            int SizeY = cv_image.Height;
            int channels = cv_image.Channels();

            MIL_ID MilImage = MIL.M_NULL;
            MIL.MbufAlloc2d(MilSystem,
                SizeX, SizeY,
                8L + MIL.M_UNSIGNED,
                MIL.M_IMAGE + MIL.M_PROC, ref MilImage);

            byte[] byteArray = new byte[SizeX * SizeY * channels];
            Marshal.Copy(cv_image.Data, byteArray, 0, byteArray.Length);

            MIL.MbufPut2d(MilImage, 0, 0, SizeX, SizeY, byteArray);

            return SetMILSrcImage(algo_id, MilImage);
        }

        public byte [] GetSrcImageToByte(int algo_id)
        {
            MIL_ID MilSrcImage = GetSrcImage(algo_id);

            if (MilSrcImage == MIL.M_NULL)
                return null;

            return GetSrcImageToByte(algo_id, MilSrcImage);
        }

        public byte[] GetSrcImageToByte(int algo_id, MIL_ID MilImage)
        {
            if (MilImage == MIL.M_NULL)
                return null;

            int width = GetWidth(MilImage);
            int height = GetHeight(MilImage);

            byte[] pixelData = new byte[width * height];

            MIL.MbufGet(MilImage, pixelData);

            return pixelData;
        }

        public MIL_ID GetSrcImage(int algo_id)
        {
            if (!MilSrcImages.ContainsKey(algo_id))
                return MIL.M_NULL;

            return MilSrcImages[algo_id];            
        }

        public MIL_ID GetGrayImage(MIL_ID MilImage)
        {
            if (MilImage == MIL.M_NULL)
                return MIL.M_NULL;

            int band = 0;

            band = (int)MIL.MbufInquire(MilImage, MIL.M_SIZE_BAND, MIL.M_NULL);

            int width = (int)MIL.MbufInquire(MilImage, MIL.M_SIZE_X);
            int height = (int)MIL.MbufInquire(MilImage, MIL.M_SIZE_Y);
            
            MIL_ID MilGrayImage;
            MilGrayImage = MIL.MbufAlloc2d(MilSystem, width, height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL);

            if (band == 1)
            {
                MIL.MbufCopy(MilImage, MilGrayImage);
            }
            else
            {
                MIL.MimConvert(MilImage, MilGrayImage, MIL.M_GRAYSCALE);
            }

            return MilGrayImage;
        }

        public bool IsValid(int algo_id, int model_id)
        {
            if (!MilSrcImages.ContainsKey(algo_id))
                return false;

            MIL_ID MilImage = MilSrcImages[algo_id];
            if (MilImage == MIL.M_NULL)
                return false;

            if (!MilModels.ContainsKey(algo_id))
                return false;

            if (!MilModels[algo_id].ContainsKey(model_id))
                return false;

            MIL_ID MilModel = MilModels[algo_id][model_id];

            if (MilModel == MIL.M_NULL)
                return false;

            return true;
        }

        public void Dispose()
        {
            ClearAll();
        }

        public void ClearAll()
        {
            if (MilSystem != MIL.M_NULL)
                MIL.MsysFree(MilSystem);

            if (MilApplication != MIL.M_NULL)
                MIL.MappFree(MilApplication);

            MilApplication = MIL.M_NULL;
            MilSystem = MIL.M_NULL;

            foreach (var mil_dic in MilModels.Values)
            {
                foreach (var model in mil_dic.Values)
                {
                    if (model != MIL.M_NULL)
                        MIL.MmodFree(model);
                }
            }

            ImageClear();
        }

        public void ImageClear()
        {
            foreach (var mil_image in MilSrcImages.Values)
            {
                if (mil_image != MIL.M_NULL)
                    MIL.MbufFree(mil_image);
            }
        }

        public int Width(int algo_id)
        {
            if (!MilSrcImages.ContainsKey(algo_id))
            {
                return 0;
            }

            return (int)MIL.MbufInquire(MilSrcImages[algo_id], MIL.M_SIZE_X);
        }

        public int Height(int algo_id)
        {
            if (!MilSrcImages.ContainsKey(algo_id))
            {
                return 0;
            }

            return (int)MIL.MbufInquire(MilSrcImages[algo_id], MIL.M_SIZE_Y);
        }

        public MIL_ID Copy(MIL_ID MilImage)
        {
            int width = (int)MIL.MbufInquire(MilImage, MIL.M_SIZE_X);
            int height = (int)MIL.MbufInquire(MilImage, MIL.M_SIZE_Y);

            MIL_ID MilTempImage;
            MilTempImage = MIL.MbufAlloc2d(MilSystem, width, height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL);

            MIL.MbufCopy(MilImage, MilTempImage);

            return MilTempImage;
        }

        public int GetWidth(MIL_ID MilImage)
        {
            return (int)MIL.MbufInquire(MilImage, MIL.M_SIZE_X);
        }

        public int GetHeight(MIL_ID MilImage)
        {
            return (int)MIL.MbufInquire(MilImage, MIL.M_SIZE_Y);
        }
        #endregion

        #region Blob Process

        // Blob
        public List<BlobData> BlobProcess(MIL_ID MilImage, int threshold, bool white = false, int NumberOfFeret = 360, int min_area = 0, int max_area = int.MaxValue)
        {
            List<BlobData> list = new List<BlobData>();

            int width = GetWidth(MilImage);
            int height = GetHeight(MilImage);

            MIL_ID MilBinImage = MIL.M_NULL;
            MilBinImage = MIL.MbufAlloc2d(MilSystem, width, height, 8L + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL);

            MIL_ID MilBlobResult = MIL.M_NULL;                  // Blob result buffer identifier.
            MIL_ID MilBlobFeatureList = MIL.M_NULL;             // Feature list identifier.

            // Allocate a feature list.
            MIL.MblobAllocFeatureList(MilSystem, ref MilBlobFeatureList);
            // Allocate a blob result buffer.
            MIL.MblobAllocResult(MilSystem, ref MilBlobResult);


            // Enable the Area and Center Of Gravity feature calculation.
            MIL.MblobSelectFeature(MilBlobFeatureList, MIL.M_AREA);
            MIL.MblobSelectFeature(MilBlobFeatureList, MIL.M_CENTER_OF_GRAVITY);

            MIL.MblobSelectFeature(MilBlobFeatureList, MIL.M_BOX_X_MIN);
            MIL.MblobSelectFeature(MilBlobFeatureList, MIL.M_BOX_Y_MIN);
            MIL.MblobSelectFeature(MilBlobFeatureList, MIL.M_BOX_X_MAX);
            MIL.MblobSelectFeature(MilBlobFeatureList, MIL.M_BOX_Y_MAX);
            MIL.MblobSelectFeature(MilBlobFeatureList, MIL.M_AREA);

            MIL.MblobSelectFeature(MilBlobFeatureList, MIL.M_SUM_PIXEL);

            MIL.MblobSelectFeature(MilBlobFeatureList, MIL.M_BOX_FILL_RATIO);



            MIL.MblobSelectFeature(MilBlobFeatureList, MIL.M_FERET_MIN_DIAMETER);
            MIL.MblobSelectFeature(MilBlobFeatureList, MIL.M_FERET_MEAN_DIAMETER);
            MIL.MblobSelectFeature(MilBlobFeatureList, MIL.M_FERET_MAX_DIAMETER);

            MIL.MblobSelectFeature(MilBlobFeatureList, MIL.M_ELONGATION);


            MIL.MblobControl(MilBlobResult, MIL.M_NUMBER_OF_FERETS, NumberOfFeret);
            MIL.MblobSelectFeature(MilBlobFeatureList, MIL.M_FERET_MIN_ANGLE);
            MIL.MblobSelectFeature(MilBlobFeatureList, MIL.M_FERET_MAX_ANGLE);


            if (white)
                MIL.MblobControl(MilBlobResult, MIL.M_FOREGROUND_VALUE, MIL.M_NON_ZERO);
            else
                MIL.MblobControl(MilBlobResult, MIL.M_FOREGROUND_VALUE, MIL.M_ZERO);

            MIL.MblobCalculate(MilBinImage, MIL.M_NULL, MilBlobFeatureList, MilBlobResult);

            MIL.MblobSelect(MilBlobResult, MIL.M_EXCLUDE, MIL.M_AREA, MIL.M_OUT_RANGE, min_area, max_area);

            //MIL_INT TotalBlobs = 0;

            int TotalBlobs = (int)MIL.MblobGetNumber(MilBlobResult, MIL.M_NULL);

            double[] CogX = new double[TotalBlobs];
            double[] CogY = new double[TotalBlobs];

            double[] MinX = new double[TotalBlobs];
            double[] MinY = new double[TotalBlobs];
            double[] MaxX = new double[TotalBlobs];
            double[] MaxY = new double[TotalBlobs];

            double[] Fill_Ratio = new double[TotalBlobs];

            double[] Area = new double[TotalBlobs];
            double[] Sum_Pixel = new double[TotalBlobs];

            double[] FeretMax = new double[TotalBlobs];
            double[] FeretMin = new double[TotalBlobs];
            double[] FeretMean = new double[TotalBlobs];

            double[] Elongation = new double[TotalBlobs];

            double[] Angle = new double[TotalBlobs];

            MIL.MblobGetResult(MilBlobResult, MIL.M_CENTER_OF_GRAVITY_X, CogX);
            MIL.MblobGetResult(MilBlobResult, MIL.M_CENTER_OF_GRAVITY_Y, CogY);

            MIL.MblobGetResult(MilBlobResult, MIL.M_ELONGATION, Elongation);

            MIL.MblobGetResult(MilBlobResult, MIL.M_BOX_X_MIN, MinX);
            MIL.MblobGetResult(MilBlobResult, MIL.M_BOX_Y_MIN, MinY);
            MIL.MblobGetResult(MilBlobResult, MIL.M_BOX_X_MAX, MaxX);
            MIL.MblobGetResult(MilBlobResult, MIL.M_BOX_Y_MAX, MaxY);
            MIL.MblobGetResult(MilBlobResult, MIL.M_AREA, Area);

            MIL.MblobGetResult(MilBlobResult, MIL.M_SUM_PIXEL, Sum_Pixel);

            MIL.MblobGetResult(MilBlobResult, MIL.M_BOX_FILL_RATIO, Fill_Ratio);

            MIL.MblobGetResult(MilBlobResult, MIL.M_FERET_MAX_DIAMETER, FeretMax);
            MIL.MblobGetResult(MilBlobResult, MIL.M_FERET_MIN_DIAMETER, FeretMin);
            MIL.MblobGetResult(MilBlobResult, MIL.M_FERET_MEAN_DIAMETER, FeretMean);
            MIL.MblobGetResult(MilBlobResult, MIL.M_FERET_MAX_ANGLE, Angle);

            for (int i = 0; i < TotalBlobs; i++)
            {
                BlobData data = new BlobData((int)MinX[i], (int)MinY[i], (int)MaxX[i], (int)MaxY[i], (int)Area[i]);

                data.CogX = (int)CogX[i];
                data.CogY = (int)CogY[i];

                list.Add(data);
            }

            MIL.MbufFree(MilBinImage);

            return list;
        }
        public List<BlobData> BlobProcess(int algo_id, int threshold, bool white = false, int NumberOfFeret = 360, int min_area = 0, int max_area = int.MaxValue)
        {
            MIL_ID MilImage = GetSrcImage(algo_id);
            if (MilImage != MIL.M_NULL)
                return new List<BlobData>();

            return BlobProcess(MilSrcImages[algo_id], threshold, white, NumberOfFeret, min_area, max_area);
        }

        #endregion

        #region Model Finder

        // Model Finder  

        public bool FindSetModel(int algo_id, int model_id, RoiRectangle roi)
        {
            MIL_ID MilImage = GetSrcImage(algo_id);
            if (MilImage == MIL.M_NULL)
                return false;

            return FindSetModel(MilImage, algo_id, model_id, roi);
        }

        public bool FindSetModel(MIL_ID MilImage, int algo_id, int model_id, RoiRectangle roi)
        {
            if (MilImage == MIL.M_NULL)
                return false;

            //SetMILSrcImage(algo_id, MilImage);

            if (!MilModels.ContainsKey(algo_id))
            {
                MilModels[algo_id] = new Dictionary<int, MIL_ID>();
            }

            MIL_ID MilModel = GetFindModelHandle(algo_id, model_id);
            if (MilModel != MIL.M_NULL)
            {
                MIL.MmodFree(MilModels[algo_id][model_id]);
                MilModel = MIL.M_NULL;
            }

            MilModel = MIL.MmodAlloc(MilSystem, MIL.M_GEOMETRIC, MIL.M_DEFAULT, MIL.M_NULL);
            //MilModels[algo_id][model_id] = MilModel;
            SetFindModelHandle(algo_id, model_id, MilModel);

            if (MilModel == MIL.M_NULL)
                return false;

            /* Define the model. */
            MIL.MmodDefine(MilModel, MIL.M_IMAGE, MilImage, roi.Left, roi.Top, roi.Width, roi.Height);

            /* Set the search speed. */
            // 디폴트 속성
            MIL.MmodControl(MilModel, MIL.M_CONTEXT, MIL.M_NUMBER, MIL.M_ALL);
            MIL.MmodControl(MilModel, MIL.M_CONTEXT, MIL.M_ACCURACY, MIL.M_HIGH);
            MIL.MmodControl(MilModel, MIL.M_CONTEXT, MIL.M_ASPECT_RATIO, MIL.M_DEFAULT);
            MIL.MmodControl(MilModel, MIL.M_CONTEXT, MIL.M_DETAIL_LEVEL, MIL.M_MEDIUM);
            MIL.MmodControl(MilModel, MIL.M_CONTEXT, MIL.M_FILTER_MODE, MIL.M_RECURSIVE);
            MIL.MmodControl(MilModel, MIL.M_CONTEXT, MIL.M_FIRST_LEVEL, MIL.M_AUTO);
            MIL.MmodControl(MilModel, MIL.M_CONTEXT, MIL.M_LAST_LEVEL, MIL.M_AUTO);
            MIL.MmodControl(MilModel, MIL.M_CONTEXT, MIL.M_SAVE_TARGET_EDGES, MIL.M_DISABLE);
            MIL.MmodControl(MilModel, MIL.M_CONTEXT, MIL.M_SEARCH_ANGLE_RANGE, MIL.M_ENABLE);
            MIL.MmodControl(MilModel, MIL.M_CONTEXT, MIL.M_SEARCH_POSITION_RANGE, MIL.M_ENABLE);
            MIL.MmodControl(MilModel, MIL.M_CONTEXT, MIL.M_SEARCH_SCALE_RANGE, MIL.M_ENABLE);
            MIL.MmodControl(MilModel, MIL.M_CONTEXT, MIL.M_SHARED_EDGES, MIL.M_DISABLE);
            MIL.MmodControl(MilModel, MIL.M_CONTEXT, MIL.M_SMOOTHNESS, MIL.M_DEFAULT);
            MIL.MmodControl(MilModel, MIL.M_CONTEXT, MIL.M_TARGET_CACHING, MIL.M_ENABLE);
            MIL.MmodControl(MilModel, MIL.M_CONTEXT, MIL.M_SPEED, MIL.M_VERY_HIGH);
            MIL.MmodControl(MilModel, MIL.M_CONTEXT, MIL.M_TIMEOUT, 1500);

            /* Preprocess the search context. */
            MIL.MmodPreprocess(MilModel, MIL.M_DEFAULT);

            return true;
        }

        public bool FindSaveModel(int algo_id, int model_id, string save_path)
        {
            MIL_ID MilModel = GetFindModelHandle(algo_id, model_id);

            if (MilModel == MIL.M_NULL)
                return false;

            MIL.MmodSave(save_path, MilModel, MIL.M_WITH_CALIBRATION);

            return true;
        }

        public bool FindLoadModel(int algo_id, int model_id, string load_path)
        {
            string extension = Path.GetExtension(load_path);

            if (extension != ".mmf")
                return false;

            MIL_ID MilModel = MIL.M_NULL;// MilModels[algo_id][model_id];

            MIL.MmodRestore(load_path, MilSystem, MIL.M_WITH_CALIBRATION, ref MilModel);


            MIL_ID MilTModel = GetFindModelHandle(algo_id, model_id);
            if (MilTModel != MIL.M_NULL)
            {
                MIL.MmodFree(MilTModel);
            }

            //MilModels[algo_id][model_id] = MilModel;
            SetFindModelHandle(algo_id, model_id, MilModel);

            return true;
        }

        public List<ModelFindData> FindModel(int algo_id, int model_id)
        {
            MIL_ID MilImage = GetSrcImage(algo_id);

            if (MilImage == MIL.M_NULL)
                return new List<ModelFindData>();

            return FindModel(MilImage, algo_id, model_id);
        }

        public List<ModelFindData> FindModel(MIL_ID MilImage, int algo_id, int model_id)
        {
            List<ModelFindData> list = new List<ModelFindData>();

            MIL_ID MilModel = GetFindModelHandle(algo_id, model_id); // MilModels[algo_id][model_id];

            if (MilImage == MIL.M_NULL || MilModel == MIL.M_NULL)
                return list;


            //if (MilImage != MIL.M_NULL)
            //{
            //    MIL.MbufExport(string.Format("D:\\MilImage_{0}_{1}.bmp", algo_id, model_id), MIL.M_BMP, MilImage);

            //}

            int MilNumResult = 0;
            int[] model = new int[MODEL_MAX_OCCURRENCES];

            double[] score = new double[MODEL_MAX_OCCURRENCES];
            double[] xpos = new double[MODEL_MAX_OCCURRENCES];
            double[] ypos = new double[MODEL_MAX_OCCURRENCES];
            double[] angle = new double[MODEL_MAX_OCCURRENCES];
            double[] scale = new double[MODEL_MAX_OCCURRENCES];

            MIL_ID MilModelResult = MIL.M_NULL;

            MIL.MmodAllocResult(MilSystem, MIL.M_GEOMETRIC, ref MilModelResult);
            /* Set the search speed. */
            MIL.MmodControl(MilModel, MIL.M_CONTEXT, MIL.M_SPEED, MIL.M_VERY_HIGH);

            /* Preprocess the search context. */
            MIL.MmodPreprocess(MilModel, MIL.M_DEFAULT);

            // 캘리브레이션 컨텍스트 ID 초기화
            MIL_INT CalibrationContextId = MIL.M_NULL;


            /* Dummy first call for bench measure purpose only (bench stabilization,
           cache effect, etc...). This first call is NOT required by the application. */
            //MmodFind(m_MilModel[nIndex], m_MilGrabImg, m_MilModelResult);
            MIL.MmodFind(MilModel, MilImage, MilModelResult);

            /* Get the number of models found. */
            MIL.MmodGetResult(MilModelResult, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref MilNumResult);

            // 중심 좌표를 실제 세계 단위로 변환
            double PixelX = 0.0;
            double PixelY = 0.0;

            /* If a model was found above the acceptance threshold. */
            if ((MilNumResult >= 1) && (MilNumResult <= MODEL_MAX_OCCURRENCES))
            {
                /* Get the results of the single model. */
                //MmodGetResult(m_MilModelResult, M_DEFAULT, M_INDEX + M_TYPE_MIL_INT, MilModel);
                //MmodGetResult(m_MilModelResult, M_DEFAULT, M_POSITION_X, MilXPos);
                //MmodGetResult(m_MilModelResult, M_DEFAULT, M_POSITION_Y, MilYPos);
                //MmodGetResult(m_MilModelResult, M_DEFAULT, M_ANGLE, MilAngle);
                //MmodGetResult(m_MilModelResult, M_DEFAULT, M_SCALE, MilScale);
                //MmodGetResult(m_MilModelResult, M_DEFAULT, M_SCORE, MilScore);

                MIL.MmodGetResult(MilModelResult, MIL.M_DEFAULT, MIL.M_INDEX + MIL.M_TYPE_MIL_INT, model);
                MIL.MmodGetResult(MilModelResult, MIL.M_DEFAULT, MIL.M_POSITION_X, xpos);
                MIL.MmodGetResult(MilModelResult, MIL.M_DEFAULT, MIL.M_POSITION_Y, ypos);
                MIL.MmodGetResult(MilModelResult, MIL.M_DEFAULT, MIL.M_ANGLE, angle);
                MIL.MmodGetResult(MilModelResult, MIL.M_DEFAULT, MIL.M_SCALE, scale);
                MIL.MmodGetResult(MilModelResult, MIL.M_DEFAULT, MIL.M_SCORE, score);

                //ResultModelFind->nCnt = MilNumResult;
                for (int i = 0; i < MilNumResult; i++)
                {
                    ModelFindData mfd = new ModelFindData();

                    // 캘리브레이션 컨텍스트 ID 출력
                    if (CalibrationContextId != MIL.M_NULL)
                    {
                        MIL.McalTransformCoordinate(MilImage, MIL.M_WORLD_TO_PIXEL, xpos[i], ypos[i], ref PixelX, ref PixelY);

                        mfd.XWorldPos = xpos[i];
                        mfd.YWorldPos = ypos[i];
                        mfd.XPos = PixelX;
                        mfd.YPos = PixelY;
                    }
                    else
                    {
                        mfd.XWorldPos = 0.0;
                        mfd.YWorldPos = 0.0;
                        mfd.XPos = xpos[i];
                        mfd.YPos = ypos[i];
                    }

                    mfd.Angle = angle[i];
                    mfd.Scale = scale[i];
                    mfd.Score = score[i];

                    list.Add(mfd);
                }
            }
            MIL.MmodFree(MilModelResult);

            return list;
        }


        public bool FindConfigDialog(int algo_id, int model_id)
        {
            MIL_ID MilModel = GetFindModelHandle(algo_id, model_id);
            if (MilModel == MIL.M_NULL)
                return false;

            MIL.MmodControl(MilModel, MIL.M_CONTEXT, MIL.M_INTERACTIVE, MIL.M_DEFAULT);

            return true;
        }


        public MIL_ID GetFindModelHandle(int algo_id, int model_id)
        {
            if (!MilModels.ContainsKey(algo_id))
                return MIL.M_NULL;

            if (!MilModels[algo_id].ContainsKey(model_id))
                return MIL.M_NULL;

            return MilModels[algo_id][model_id];
        }

        public bool SetFindModelHandle(int algo_id, int model_id, MIL_ID MilModel)
        {
            MIL_ID MilTModel = GetFindModelHandle(algo_id, model_id);
            if (MilTModel != MIL.M_NULL)
            {
                MIL.MmodFree(MilTModel);
            }

            if (!MilModels.ContainsKey(algo_id))
                MilModels[algo_id] = new Dictionary<int, MIL_ID>();

            MilModels[algo_id][model_id] = MilModel;

            return true;
        }

        #endregion

        #region Calibration

        // Calibration
        public CalibrationData GridCalibration(int algo_id, int cal_id,
            double grid_offset_x, double grid_offset_y, double grid_offset_z,
            int row, int col, double row_spacing, double col_spacing, int operation, int grid_type)
        {
            MIL_ID MilImage = GetSrcImage(algo_id);
            if (MilImage == MIL.M_NULL)
                return null;

            return GridCalibration(MilImage, algo_id, cal_id, grid_offset_x, grid_offset_y, grid_offset_z,
                row, col, row_spacing, col_spacing, operation, grid_type);
        }

        public CalibrationData GridCalibration(MIL_ID MilImage, int algo_id, int cal_id,
            double grid_offset_x, double grid_offset_y, double grid_offset_z,
            int row, int col, double row_spacing, double col_spacing, int operation, int grip_type)
        {
            if (MilImage == MIL.M_NULL)
                return null;

            MIL_ID MilCal = GetCalibrationHandle(algo_id, cal_id); //
            if (MilCal != MIL.M_NULL)
            {
                MIL.McalFree(MilGridCalibration[algo_id][cal_id]);
                MilGridCalibration[algo_id][cal_id] = MIL.M_NULL;
            }

            MilCal = MIL.McalAlloc(MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_NULL);
            //MilGridCalibration[algo_id][cal_id] = MilCal;
            SetCalibrationHandle(algo_id, cal_id, MilCal);

            CalibrationData data = new CalibrationData();

            // 이미지와 캘리브레이션 컨텍스트의 연결 해제
            MIL.McalAssociate(MIL.M_NULL, MilImage, MIL.M_DEFAULT);

            /* Calibrate the camera with the image of the grid and its world description. */
            MIL.McalGrid(MilCal, MilImage,
                grid_offset_x, grid_offset_y, grid_offset_z,
                row, col,
                row_spacing, col_spacing,
                MIL.M_DEFAULT, grip_type);  // nGridType - 1 : Circle, 2 : Chessboard

            if (!GetCalibrationStatus(algo_id, cal_id))
                return null;

            // 이미지 크기 가져오기
            double ImageWidth = (double)MIL.MbufInquire(MilImage, MIL.M_SIZE_X, MIL.M_NULL);
            double ImageHeight = (double)MIL.MbufInquire(MilImage, MIL.M_SIZE_Y, MIL.M_NULL);

            // 이미지 중심 계산
            double CenterX = ImageWidth / 2.0;
            double CenterY = ImageHeight / 2.0;

            // 사각형 지역의 넓이와 높이를 실제 세계 단위로 변환
            double RealWorldWidth = 0.0;
            double RealWorldHeight = 0.0;

            MIL.McalTransformCoordinate(MilImage, MIL.M_PIXEL_TO_WORLD, CenterX, CenterY, ref RealWorldWidth, ref RealWorldHeight);

            // 캘리브레이션 컨텍스트에 새로운 원점 적용
            MIL.McalControl(MilCal, MIL.M_CALIBRATION_PLANE, MIL.M_RELATIVE_COORDINATE_SYSTEM);
            MIL.McalControl(MilCal, MIL.M_TOOL_POSITION_X, -RealWorldWidth);
            MIL.McalControl(MilCal, MIL.M_TOOL_POSITION_Y, -RealWorldHeight);

            // 픽셀 좌표를 실제 세계 좌표계로 변환
            double WorldX0 = 0.0;
            double WorldY0 = 0.0;
            double WorldX1 = 0.0;
            double WorldY1 = 0.0;
            MIL.McalTransformCoordinate(MilCal, MIL.M_PIXEL_TO_WORLD, 0, 0, ref WorldX0, ref WorldY0);
            MIL.McalTransformCoordinate(MilCal, MIL.M_PIXEL_TO_WORLD, 1, 1, ref WorldX1, ref WorldY1);

            // 픽셀당 실제 거리 계산
            data.DistanceX = (double)(WorldX1 - WorldX0);
            data.DistanceY = (double)(WorldY1 - WorldY0);
            return data;
        }
        
        public bool GridCalibrationAssociate(int algo_id, int cal_id)
        {
            MIL_ID MilImage = GetSrcImage(algo_id);
            if (MilImage == MIL.M_NULL)
                return false;

            return GridCalibrationAssociate(MilImage, algo_id, cal_id);
        }

        public bool GridCalibrationAssociate(MIL_ID MilImage, int algo_id, int cal_id)
        {
            if (MilImage == MIL.M_NULL)
                return false;

            if (!GetCalibrationStatus(algo_id, cal_id))
                return false;

            MIL_ID MilCal = MilGridCalibration[algo_id][cal_id];

            //Calibration associate
            MIL.McalAssociate(MilCal, MilImage, MIL.M_DEFAULT);

            return true;
        }

        public bool GridCalibrationDeAssociate(int algo_id, int cal_id)
        {
            MIL_ID MilImage = GetSrcImage(algo_id);
            if (MilImage == MIL.M_NULL)
                return false;

            return GridCalibrationDeAssociate(MilImage, algo_id, cal_id);
        }

        public bool GridCalibrationDeAssociate(MIL_ID MilImage, int algo_id, int cal_id)
        {
            if (MilImage == MIL.M_NULL)
                return false;

            MIL.McalAssociate(MIL.M_NULL, MilImage, MIL.M_DEFAULT);

            return true;
        }
        
        public bool GridCalibrationModelAssociate(int algo_id, int model_id)
        {
            MIL_ID MilImage = GetSrcImage(algo_id);
            MIL_ID MilModel = GetFindModelHandle(algo_id, model_id);

            if (MilImage == MIL.M_NULL || MilModel == MIL.M_NULL)
                return false;

            MIL_ID CalibrationContextId = MIL.M_NULL;

            // 이미지에 연관된 캘리브레이션 컨텍스트 ID 얻기
            MIL.McalInquire(MilImage, MIL.M_ASSOCIATED_CALIBRATION, ref CalibrationContextId);
            if (CalibrationContextId == MIL.M_NULL)
            {
                return false;
            }
            //Calibration associate
            MIL.McalAssociate(CalibrationContextId, MilModel, MIL.M_DEFAULT);
            return true;

        }

        public bool GridCalibrationModelDeAssociate(int algo_id, int model_id)
        {
            MIL_ID MilModel = GetFindModelHandle(algo_id, model_id);
            if (MilModel == MIL.M_NULL)
                return false;

            //Calibration deassociate
            MIL.McalAssociate(MIL.M_NULL, MilModel, MIL.M_DEFAULT);

            return true;
        }

        public DPointCoordinates GridCalibrationTransformCoordinatesToWorld(int algo_id, int cal_id, double x, double y)
        {
            MIL_ID MilImage = GetSrcImage(algo_id);

            if (MilImage == MIL.M_NULL)
                return null;

            return GridCalibrationTransformCoordinatesToWorld(MilImage, algo_id, cal_id, x, y);
        }
        
        public DPointCoordinates GridCalibrationTransformCoordinatesToWorld(MIL_ID MilImage, int algo_id, int cal_id, double x, double y)
        {
            if (GetCalibrationStatus(algo_id, cal_id) != true)
                return null;

            double tx = 0.0;
            double ty = 0.0;
            /* Transform coordinate. */
            MIL.McalTransformCoordinate(MilImage, MIL.M_PIXEL_TO_WORLD, x, y, ref tx, ref ty);

            DPointCoordinates point = new DPointCoordinates(tx, ty);

            return point;
        }

        public bool GridCalibrationChangeOriginWorldCoordinate(int algo_id, int cal_id, int pixelx, int pixely)
        {
            MIL_ID MilImage = GetSrcImage(algo_id);

            if (MilImage == MIL.M_NULL)
                return false;

            return GridCalibrationChangeOriginWorldCoordinate(MilImage, algo_id, cal_id, pixelx, pixely);
        }

        public bool GridCalibrationChangeOriginWorldCoordinate(MIL_ID MilImage, int algo_id, int cal_id, int pixelx, int pixely)
        {
            if (MilImage == MIL.M_NULL)
                return false;

            MIL_ID MilCal = GetCalibrationHandle(algo_id, cal_id);

            if (MilCal == MIL.M_NULL)
                return false;

            double RealWorldWidth = 0.0;
            double RealWorldHeight = 0.0;

            //MIL.McalTransformCoordinate(MilImage, MIL.M_PIXEL_TO_WORLD, 0, 0, ref RealWorldWidth, ref RealWorldHeight);

            // 캘리브레이션 컨텍스트에 새로운 원점 적용
            MIL.McalControl(MilCal, MIL.M_CALIBRATION_PLANE, MIL.M_RELATIVE_COORDINATE_SYSTEM);
            MIL.McalControl(MilCal, MIL.M_TOOL_POSITION_X, 0);
            MIL.McalControl(MilCal, MIL.M_TOOL_POSITION_Y, 0);

            MIL.McalTransformCoordinate(MilImage, MIL.M_PIXEL_TO_WORLD, 0, 0, ref RealWorldWidth, ref RealWorldHeight);


            MIL.McalTransformCoordinate(MilImage, MIL.M_PIXEL_TO_WORLD, pixelx, pixelx, ref RealWorldWidth, ref RealWorldHeight);

            // 캘리브레이션 컨텍스트에 새로운 원점 적용
            MIL.McalControl(MilCal, MIL.M_CALIBRATION_PLANE, MIL.M_RELATIVE_COORDINATE_SYSTEM);
            MIL.McalControl(MilCal, MIL.M_TOOL_POSITION_X, -RealWorldWidth);
            MIL.McalControl(MilCal, MIL.M_TOOL_POSITION_Y, -RealWorldHeight);


            return true;
        }

        public bool SaveGridCalibration(int algo_id, int cal_id, string save_path)
        {
            MIL_ID MilCal = GetCalibrationHandle(algo_id, cal_id);
            if (MilCal == MIL.M_NULL)
                return false;

            //Calibration save
            MIL.McalSave(save_path, MilCal, MIL.M_DEFAULT);

            return true;
        }

        public bool LoadGridCalibration(int algo_id, int cal_id, string load_path)
        {
            MIL_ID MilCal = GetCalibrationHandle(algo_id, cal_id);
            if (MilCal != MIL.M_NULL)
            {
                MIL.McalFree(MilGridCalibration[algo_id][cal_id]);
                MilGridCalibration[algo_id][cal_id] = MIL.M_NULL;
            }
            //Calibration load
            MIL_ID mRet = MIL.McalRestore(load_path, MilSystem, MIL.M_DEFAULT, ref MilCal);

            if (mRet == MIL.M_NULL)
            {
                return false;
            }
            if(!MilGridCalibration.ContainsKey(algo_id))
            {
                MilGridCalibration[algo_id] = new Dictionary<int, MIL_ID>();
            }
            MilGridCalibration[algo_id][cal_id] = MilCal;

            if (!GetCalibrationStatus(algo_id, cal_id))
            {
                MilGridCalibration[algo_id][cal_id] = MIL.M_NULL;
                return false;
            }
            return true;
        }

        public bool GetCalibrationStatus(int algo_id, int cal_id)
        {
            MIL_ID MilCal = GetCalibrationHandle(algo_id, cal_id);
            if (MilCal == MIL.M_NULL)
                return false;

            int status = 0;
            MIL.McalInquire(MilCal, MIL.M_CALIBRATION_STATUS + MIL.M_TYPE_MIL_INT, ref status);

            if (status == MIL.M_CALIBRATED)
                return true;

            return false;
        }

        public MIL_ID GetCalibrationHandle(int algo_id, int cal_id)
        {
            if (!MilGridCalibration.ContainsKey(algo_id) ||
                    !MilGridCalibration[algo_id].ContainsKey(cal_id) ||
                    MilGridCalibration[algo_id][cal_id] == MIL.M_NULL)
                return MIL.M_NULL;

            return MilGridCalibration[algo_id][cal_id];
        }

        public bool SetCalibrationHandle(int algo_id, int cal_id, MIL_ID MilCal)
        {
            if (!MilGridCalibration.ContainsKey(algo_id))
            {
                MilGridCalibration[algo_id] = new Dictionary<int, MIL_ID>();
                MilGridCalibration[algo_id][cal_id] = MilCal;

                return true;
            }
            else
            {
                if(!MilGridCalibration[algo_id].ContainsKey(cal_id))
                {
                    MilGridCalibration[algo_id][cal_id] = MilCal;
                    return true;
                }

                if (MilGridCalibration[algo_id][cal_id] == MIL.M_NULL)
                {
                    MilGridCalibration[algo_id][cal_id] = MilCal;
                    return true;
                }

                MIL.McalFree(MilGridCalibration[algo_id][cal_id]);
                MilGridCalibration[algo_id][cal_id] = MilCal;
                return true;
            }



            
        }
        #endregion

        #region OrientSearch
        public List<OrientData> OrientSearch(int algo_id, int search_count, bool image_align)
        {
            MIL_ID MilSrcImage = GetSrcImage(algo_id);
            if (MilSrcImage == MIL.M_NULL)
                return new List<OrientData>();

            return OrientSearch(MilSrcImage, algo_id, search_count, image_align);

        }

        public List<OrientData> OrientSearch(MIL_ID MilSrcImage, int algo_id, int search_count, bool image_align)
        {
            if (MilSrcImage == MIL.M_NULL)
                return new List<OrientData>();

            const int ORIENT_MAX_COUNT = 32;

            MIL_ID MilImage = MIL.M_NULL;          /* Image buffer identifier.                 */
            MIL_ID MilResultId = MIL.M_NULL;       /* Result buffer identifier.                */

            MIL_INT SizeX, SizeY;
            //MIL_FLOAT Orientations[ORIENT_MAX_COUNT];  /*  Orientations  */
            //MIL_FLOAT Score[ORIENT_MAX_COUNT];         /*  Scores        */
            double[] Orientations = new double[ORIENT_MAX_COUNT];
            double[] Score = new double[ORIENT_MAX_COUNT];


            double[] ResultAngle = new double[ORIENT_MAX_COUNT];
            double[] ResultScore = new double[ORIENT_MAX_COUNT];

            /* Inquire the image Size and Type. */
            SizeX = GetWidth(MilSrcImage);
            SizeY = GetHeight(MilSrcImage);

            /* Allocate a display buffer and clear it. */
            MIL.MbufAlloc2d(MilSystem, SizeX, SizeY, 8L + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP, MilImage);
            MIL.MbufClear(MilImage, 0L);

            /*Allocate the Result buffer */
            MIL.MimAllocResult(MilSystem, search_count, MIL.M_FIND_ORIENTATION_LIST + MIL.M_FLOAT, MilResultId);

            /* Load a noisy image. */
            MIL.MbufCopy(MilSrcImage, MilImage);

            /* Find the main orientation of the image. */
            MIL.MimFindOrientation(MIL.M_DEFAULT, MilImage, MilResultId, MIL.M_DEFAULT);


            MIL.MimGetResult1d(MilResultId, 0, search_count, MIL.M_ANGLE, Orientations);
            MIL.MimGetResult1d(MilResultId, 0, search_count, MIL.M_SCORE, Score);

            List<OrientData> list = new List<OrientData>();
            for (int i = 0; i < search_count; i++)
            {
                OrientData data = new OrientData(Orientations[i], Score[i]);
                list.Add(data);
            }

            /* Free buffers. */
            MIL.MbufFree(MilImage);
            MIL.MimFree(MilResultId);

            if(image_align == true)
            {

                MIL_ID MilAlignImage = MIL.M_NULL;              /* Image buffer identifier.                 */
                MIL_ID MilSubImage00 = MIL.M_NULL;       /* Child buffer identifier.                 */
                MIL_ID MilSubImage01 = MIL.M_NULL;         /* Child buffer identifier.                 */
                MIL_ID MilWarpMatrix = MIL.M_NULL;        /* Warp matrix identifier.                  */
                MIL_ID MilAlignResultId = MIL.M_NULL;           /* Result buffer identifier.                */

                int AlignSizeX = 0;
                int AlignSizeY = 0;

                double Orientation = 0.0;
                double CorrectedOrientation = 0.0; /* Orientation of the image */

                /* Inquire the image Size and Type. */
                AlignSizeX = GetWidth(MilSrcImage);
                AlignSizeY = GetHeight(MilSrcImage);

                /* Allocate a display buffer and clear it. */
                MIL.MbufAlloc2d(MilSystem, 2 * SizeX, SizeY, 8L + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP, MilAlignImage);
                MIL.MbufClear(MilImage, 0L);

                /* Allocate child buffers in the 4 quadrants of the display image. */
                MIL.MbufChild2d(MilImage, 0L, 0L, AlignSizeX, AlignSizeY, MilSubImage00);
                MIL.MbufChild2d(MilImage, AlignSizeX, 0L, AlignSizeX, AlignSizeY, MilSubImage01);

                /*Allocate the Result buffer. */
                MIL.MimAllocResult(MilSystem, 1L, MIL.M_FIND_ORIENTATION_LIST + MIL.M_FLOAT, MilAlignResultId);

                /* Allocate the warp matrix. */
                MIL.MbufAlloc2d(MilSystem, 3L, 3L, MIL.M_FLOAT + 32, MIL.M_ARRAY, MilWarpMatrix);

                /* Load a noisy image. */
                MIL.MbufCopy(MilSrcImage, MilImage);
                MIL.MbufClear(MilSubImage01, MIL.M_COLOR_WHITE);

                /* Find the main orientation of the image. */
                MIL.MimFindOrientation(MIL.M_DEFAULT, MilSubImage00, MilResultId, MIL.M_DEFAULT);

                /* Get the result from the result buffer. */
                MIL.MimGetResult1d(MilResultId, 0L, 1L, MIL.M_ANGLE, ref Orientation);

                /* Evaluate shortest rotation to horizontal alignment. */
                CorrectedOrientation = (Orientation < 180 - Orientation) ? -Orientation : 180 - Orientation;

                /* Generate the warp matrix */
                MIL.MgenWarpParameter(MIL.M_NULL, MilWarpMatrix, MIL.M_NULL, MIL.M_WARP_POLYNOMIAL, MIL.M_TRANSLATE, (double)(-AlignSizeX / 2), (double)(-AlignSizeX / 2));
                MIL.MgenWarpParameter(MilWarpMatrix, MilWarpMatrix, MIL.M_NULL, MIL.M_WARP_POLYNOMIAL, MIL.M_ROTATE, CorrectedOrientation, MIL.M_NULL);
                MIL.MgenWarpParameter(MilWarpMatrix, MilWarpMatrix, MIL.M_NULL, MIL.M_WARP_POLYNOMIAL, MIL.M_TRANSLATE, (double)(AlignSizeX / 2), (double)(AlignSizeX / 2));

                /* Warp the image to correct the orientation. */
                MIL.MimWarp(MilSubImage00, MilSubImage01, MilWarpMatrix, MIL.M_NULL, MIL.M_WARP_POLYNOMIAL, MIL.M_BICUBIC);


                //MIL.MosGetch();

                /* Free buffers. */
                MIL.MbufFree(MilSubImage00);
                MIL.MbufFree(MilSubImage01);
                MIL.MbufFree(MilAlignImage);
                MIL.MbufFree(MilWarpMatrix);
                MIL.MimFree(MilAlignResultId);
            }

            return list;
        }
        #endregion

        #region Ransac
        public CircleData SearchRansacCircle(int algo_id, double SearchRange, double Step, int BrightThreshold, int SkipInitBright, int DistThreshold, int NoSample,
            bool SearchDir, int PreCenterX, int PreCenterY, int PreRadius)
        {
            MIL_ID MilSrcImage = GetSrcImage(algo_id);
            if (MilSrcImage == MIL.M_NULL)
                return null;

            return SearchRansacCircle(MilSrcImage, algo_id, SearchRange, Step, BrightThreshold, SkipInitBright, DistThreshold, NoSample,
                SearchDir, PreCenterX, PreCenterY, PreRadius);
        }

        public CircleData SearchRansacCircle(MIL_ID MilImage, int algo_id, double SearchRange, double Step, int BrightThreshold, int SkipInitBright, int DistThreshold, int NoSample,
            bool SearchDir, int PreCenterX, int PreCenterY, int PreRadius)
        {
            byte[] byte_image = GetSrcImageToByte(algo_id, MilImage);
            if (byte_image == null)
                return null;

            List<List<DPointCoordinates>> list = new List<List<DPointCoordinates>>();

            SearchRange /= 100.0;
            int idx = 0;
            int nBright = 0;
            int nBright_next = 0;
            double dStart = 0.0;
            double dEnd = 0.0;
            double dAngle = 0.0;


            if (SearchDir == true)     /* Out → In */
            {
                dStart = 1.0 + SearchRange;
                dEnd = 1.0 - SearchRange;
            }
            else         /* In → Out */
            {
                dStart = 1.0 - SearchRange;
                dEnd = 1.0 + SearchRange;
            }

            List<List<DPointCoordinates>> vert_lines = LineDonut(PreCenterX, PreCenterY, PreRadius, dStart, dEnd, Step);// new List<List<DPointCoordinates>>();

            int width = GetWidth(MilImage);
            int height = GetHeight(MilImage);
            bool bFirst = true;


            List<DPointCoordinates> structPoData = new List<DPointCoordinates>();

            foreach (var vert_line in vert_lines)
            {
                nBright = 0;
                nBright_next = 0;

                if (vert_line.Count < 2)
                    break;

                for(int i = 0; i < vert_line.Count -1; i++)
                {
                    var calpoint = vert_line[i];
                    var next_calpoint = vert_line[i + 1];

                    if (calpoint.X < 0 || calpoint.X >= width || calpoint.Y < 0 || calpoint.Y >= height)
                        continue;

                    if (next_calpoint.X < 0 || next_calpoint.X >= width || next_calpoint.Y < 0 || next_calpoint.Y >= height)
                        continue;


                    nBright = byte_image[(int)(width * (int)calpoint.Y + (int)calpoint.X)];
                    nBright_next = byte_image[(int)(width * (int)next_calpoint.Y + (int)next_calpoint.X)];

                    if (bFirst == true && nBright < SkipInitBright)
                    {
                        break;
                    }

                    bFirst = false;

                    int nDifference = 0;

                    if (SearchDir == true)     /* Out → In */
                    {
                        nDifference = nBright - nBright_next;
                    }
                    else         /* In → Out */
                    {
                        nDifference = nBright_next - nBright;
                    }

                    if (nDifference > BrightThreshold)
                    {
                        PointCoordinates cPoint = new PointCoordinates((int) calpoint.X, (int) calpoint.Y);
                        DPointCoordinates spoint = new DPointCoordinates();
                        spoint.X = cPoint.X;
                        spoint.Y = cPoint.Y;
                        structPoData.Add(spoint);

                        break;
                    }
                }
                dAngle += Step;
            }

            CircleData circle_data = new CircleData();

            double cost = ransac_circle_fitting(structPoData, structPoData.Count, circle_data, DistThreshold, NoSample);


            return circle_data;
        }

        protected List<List<DPointCoordinates>> LineDonut(double center_x, double center_y, double radius, double start_ratio, double end_ratio, double step_angle)
        {
            List<List<DPointCoordinates>> combine_vertical_xy = new List<List<DPointCoordinates>>();
            if (radius == 0)
            {
                return combine_vertical_xy;
            }

            if (step_angle <= 0)
            {
                return combine_vertical_xy;
            }

            if (start_ratio <= 0 && end_ratio <= 0)
            {
                return combine_vertical_xy;
            }

            int start_radius = (int)((double)radius * start_ratio);
            int end_radius = (int)((double)radius * end_ratio);

            int sign = 1;
            if (start_radius > end_radius)
                sign = -1;

            for (double angle = 0; angle < 360;)
            {
                List<DPointCoordinates> vertical_xy = new List<DPointCoordinates>();

                for (int nRad = start_radius; sign == 1 ? nRad < end_radius : nRad > end_radius; nRad += sign)
                {
                    DPointCoordinates calpoint = new DPointCoordinates();
                    calpoint.X = center_x + nRad * Math.Cos(angle * Math.PI / 180.0);
                    calpoint.Y = center_y + nRad * Math.Sin(angle * Math.PI / 180.0);

                    vertical_xy.Add(calpoint);
                }

                combine_vertical_xy.Add(vertical_xy);

                angle += step_angle;
            }

            return combine_vertical_xy;
        }

        protected double ransac_circle_fitting(List<DPointCoordinates> data, int no_data, CircleData model, double distance_threshold, int no_samples)
        {
            if (no_data < no_samples)
            {
                return 0.0;
            }

            

            int no_inliers = 0;
            List<DPointCoordinates> inliers = new List<DPointCoordinates>();

            CircleData estimated_model = new CircleData();
            double max_cost = 0.0;

            int max_iteration = (int)(1 + Math.Log(1.0 - 0.99) / Math.Log(1.0 - Math.Pow(0.5, no_samples)));

            for (int i = 0; i < max_iteration; i++)
            {
                List<DPointCoordinates> samples = new List<DPointCoordinates>();
                // 1. hypothesis

                // 원본 데이터에서 임의로 N개의 셈플 데이터를 고른다.
                get_samples(samples, no_samples, data, no_data);

                // 이 데이터를 정상적인 데이터로 보고 모델 파라메터를 예측한다.
                compute_model_parameter(samples, no_samples, estimated_model);

                // 2. Verification

                // 원본 데이터가 예측된 모델에 잘 맞는지 검사한다.
                double cost = model_verification(inliers, ref no_inliers, estimated_model, data, no_data, distance_threshold);

                // 만일 예측된 모델이 잘 맞는다면, 이 모델에 대한 유효한 데이터로 새로운 모델을 구한다.
                if (max_cost < cost)
                {
                    max_cost = cost;

                    compute_model_parameter(inliers, no_inliers, model);
                }
            }


            return max_cost;
        }

        bool find_in_samples(List<DPointCoordinates> samples, int no_samples, DPointCoordinates data)
        {
            for (int i = 0; i < samples.Count; ++i)
            {
                if (samples[i].X == data.X && samples[i].Y == data.Y)
                {
                    return true;
                }
            }
            return false;
        }

        protected void get_samples(List<DPointCoordinates> samples, int no_samples, List<DPointCoordinates> data, int no_data)
        {
            // 데이터에서 중복되지 않게 N개의 무작위 셈플을 채취한다.
            for (int i = 0; i < no_samples;)
            {
                Random rnd = new Random();
                int j = rnd.Next() % no_data;

                if (!find_in_samples(samples, i, data[j]))
                {
                    //samples[i] = data[j];
                    samples.Add(data[j]);

                    ++i;
                }
            };
        }

        protected int compute_model_parameter(List<DPointCoordinates> samples, int no_samples, CircleData model)
        {
            //// 중심 (a,b), 반지름 c인 원의 방정식: (x - a)^2 + (y - b)^2 = c^2
            //// 식을 전개하면: x^2 + y^2 - 2ax - 2by + a^2 + b^2 - c^2 = 0
            //dMatrix A(no_samples, 3);
            //dMatrix B(no_samples, 1);
            Matrix<double> A = Matrix<double>.Build.Dense(no_samples, 3);
            Matrix<double> B = Matrix<double>.Build.Dense(no_samples, 1);
                       
            for (int i = 0; i < no_samples; i++)
            {
                double x = samples[i].X;
                double y = samples[i].Y;

                A[i, 0] = x; // Math.NET은 0-based indexer 사용
                A[i, 1] = y;
                A[i, 2] = 1.0;

                B[i, 0] = -x * x - y * y;
            }

            //// AX=B 형태의 해를 least squares solution으로 구하기 위해
            //// Moore-Penrose pseudo-inverse를 이용한다.
            //dMatrix invA = !(~A * A) * ~A;
            //dMatrix X = invA * B;

            Matrix<double> A_transpose = A.Transpose();
            Matrix<double> A_t_times_A = A_transpose * A;

            Matrix<double> invA_t_times_A;
            try
            {
                invA_t_times_A = A_t_times_A.Inverse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Matrix is singular. Cannot compute inverse. {ex.Message}");
                return 0; // 실패 반환
            }

            Matrix<double> invA = invA_t_times_A * A_transpose; // Moore-Penrose pseudo-inverse
            Matrix<double> X = invA * B;

            double x0 = X[0, 0]; // C++의 X(0,0)
            double x1 = X[1, 0]; // C++의 X(1,0)
            double x2 = X[2, 0]; // C++의 X(2,0)

            // 중심 (cx, cy)와 반지름 (r) 계산
            double cx = -x0 / 2.0;
            double cy = -x1 / 2.0;
            double radiusSquared = cx * cx + cy * cy - x2;

            if (radiusSquared < 0 || double.IsNaN(radiusSquared) == true) //radiusSquared == double.NaN)
            {
                // 계산된 반지름의 제곱이 음수일 경우, 실제 원이 아님 (수치적 오차 또는 데이터 문제)
                return 0; // 실패 반환
            }

            model.X = cx;
            model.Y = cy;
            model.R = Math.Sqrt(radiusSquared); // C#의 제곱근 함수

            return 1;
        }

        protected double model_verification(List<DPointCoordinates> inliers, ref int no_inliers, CircleData estimated_model, List<DPointCoordinates> data, int no_data, double distance_threshold)
        {
            double cost = 0.0;


            no_inliers = 0;
            for (int i = 0; i < no_data; i++)
            {
                // 직선에 내린 수선의 길이를 계산한다.
                double distance = compute_distance(estimated_model, data[i]);

                // 예측된 모델에서 유효한 데이터인 경우, 유효한 데이터 집합에 더한다.
                if (distance < distance_threshold)
                {
                    cost += 1.0;

                    inliers.Add(data[i]);
                    ++no_inliers;
                }
            }

            return cost;
        }


        double compute_distance(CircleData circle, DPointCoordinates x)
        {
            // 원의 둘레로부터 떨어진 거리를 계산한다.
            // 즉, 점 x와 원의 중심 까지의 거리를 구해서 원의 반지름을 뺀다.

            double dx = circle.X - x.X;
            double dy = circle.Y - x.Y;

            return Math.Abs(Math.Sqrt(dx * dx + dy * dy) - circle.R);
        }

        #endregion

        #region constructors
        protected MilManager()
        {
            // Allocate defaults.

            MIL.MappAllocDefault(MIL.M_DEFAULT, ref MilApplication, ref MilSystem, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL);
            MIL.MappControl(MIL.M_ERROR, MIL.M_PRINT_DISABLE);

            //MIL.MappAlloc(MIL.M_DEFAULT, MilApplication);
            //MIL.MappControl(MIL.M_ERROR, MIL.M_PRINT_DISABLE);
            //MIL.MsysAlloc("M_DEFAULT", MIL.M_DEFAULT, MIL.M_DEFAULT, MilSystem);


            MilSrcImages = new Dictionary<int, MIL_ID>();

            // Model Finder
            MilModels = new Dictionary<int, Dictionary<int, MIL_ID>>();

            // Calibration
            MilGridCalibration = new Dictionary<int, Dictionary<int, MIL_ID>>();
        }
        #endregion
    }
}
