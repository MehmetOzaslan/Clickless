

import uvicorn
import PIL.Image as Image
import json
from fastapi import FastAPI, WebSocket
import io
import cv2
import numpy as np
from sklearn.cluster import DBSCAN
import vision_backend

app = FastAPI()



@app.websocket("/echotext")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    while True:
        data = await websocket.receive_text()
        print(f"Recieved following data {data}")
        await websocket.send_text(f"{data}")
        print(f"Sent Data")

@app.websocket("/echobytes")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    while True:
        data = await websocket.receive_bytes()
        print(f"Recieved following data {data}")
        await websocket.send_bytes(f"{data}")
        print(f"Sent Data")

@app.websocket("/imagesave")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    while True:
        data = await websocket.receive_bytes()
        print(f"Recieved following data {data}")
        image = Image.open(io.BytesIO(data))
        image.save("imagesave_endpoint.jpg")
        print(f"saved image")


@app.websocket("/imageml")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    while True:
        data = await websocket.receive_bytes()
        print("Recieved data")
        img_stream = io.BytesIO(data)
        image = cv2.imdecode(np.frombuffer(img_stream.read(), np.uint8), cv2.IMREAD_GRAYSCALE)
        bboxes = vision_backend.get_bboxes(image)
        
        def np_encoder(object):
            if isinstance(object, np.generic):
                return object.item()

        out_data = json.dumps(bboxes, default=np_encoder)
        await websocket.send_text(out_data)

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8210)
    