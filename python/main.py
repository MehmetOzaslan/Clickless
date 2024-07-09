

import uvicorn
import PIL.Image as Image
from fastapi import FastAPI, WebSocket
import io
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

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8210)