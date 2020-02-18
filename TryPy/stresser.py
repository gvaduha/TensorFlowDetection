import time
import cv2 as cv
import tensorflow as tf
from flask import Flask
import json
import argparse

parser = argparse.ArgumentParser()

parser.add_argument('-m', action='store', dest='model_url', help='Model graph url', required=True)
parser.add_argument('-v', action='store', dest='video_url', help='Stresser video url', required=True)
args = parser.parse_args()

classNames = {  1: 'class#1', 2: 'class#2', 3: 'class#3', 4: 'class#4', 5: 'class#5', 6: 'class#6', 7: 'class#7' }

with tf.gfile.FastGFile(args.model_url, 'rb') as f:
    graph = tf.GraphDef()
    graph.ParseFromString(f.read())

session = tf.Session() #with tf.Session() as session:
session.graph.as_default()
tf.import_graph_def(graph, name='')

video = cv.VideoCapture(args.video_url)

app = Flask(__name__)

@app.route('/')
def serve():
    ret, img = video.read()
    inp = img[:, :, [2, 1, 0]]  # BGR2RGB
    start_time = time.time()
    out = session.run([session.graph.get_tensor_by_name('num_detections:0'),
                    session.graph.get_tensor_by_name('detection_scores:0'),
                    session.graph.get_tensor_by_name('detection_boxes:0'),
                    session.graph.get_tensor_by_name('detection_classes:0')],
                   feed_dict={'image_tensor:0': inp.reshape(1, inp.shape[0], inp.shape[1], 3)})
    print("FPS: ", 1.0 / (time.time() - start_time))

    num_detections = int(out[0][0])
    outjson = []
    for i in range(num_detections):
        classId = int(out[3][0][i])
        score = float(out[1][0][i])
        bbox = [float(v) for v in out[2][0][i]]
        outjson.append({"class":classId,"score":score,"bbox":bbox})

    return json.dumps(outjson)

app.run()


