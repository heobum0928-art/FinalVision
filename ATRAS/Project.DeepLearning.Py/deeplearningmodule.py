import numpy as np

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
