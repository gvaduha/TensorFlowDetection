import cv2 as cv
import requests
import json
import argparse

parser = argparse.ArgumentParser()

parser.add_argument('-s', action='store', dest='host_url', default="http://localhost:5000/", help='Stresser host url. Default is http://localhost:5000/')
parser.add_argument('-v', action='store', dest='video_url', help='Stresser video url', required=True)
args = parser.parse_args()

print(args.host_url)
print(args.video_url)


def draw_boxes(color, bbox, cols, rows, img, classNames, classId, score):
    x = bbox[1] * cols
    y = bbox[0] * rows
    right = bbox[3] * cols
    bottom = bbox[2] * rows
    cv.rectangle(img, (int(x), int(y)), (int(right), int(bottom)), color, thickness=2)
    label = classNames[classId] + ": " + str(round(score, 1))
    labelSize, baseLine = cv.getTextSize(label, cv.FONT_HERSHEY_SIMPLEX, 0.5, 1)
    yLeftBottom = max(int(y), labelSize[1])
    cv.rectangle(img, (int(x), yLeftBottom - labelSize[1]),
                 (int(x) + labelSize[0], yLeftBottom + baseLine),
                 color, cv.FILLED)
    cv.putText(img, label, (int(x), yLeftBottom), cv.FONT_HERSHEY_SIMPLEX, 0.5, (0, 0, 0))



video = cv.VideoCapture(args.video_url)

try:
    while True:
        r = requests.get(url = args.host_url)
        data = r.json()
        ret, img = cap.read()
        rows = img.shape[0]
        cols = img.shape[1]

        for d in data:
            score = d["score"]
            classId = d["class"]
            bbox = d["bbox"]
            if score >= 0.8:
                boxcolor = (10, 10, 255)
            else
                boxcolor = (10, 10, 55)
            if score >= 0.3:
                utils.draw_boxes(boxcolor, bbox, cols, rows, img, classNames, classId, score)
            cv.imshow('result', img)
            print("Pos: ",  cap.get(cv.CAP_PROP_POS_MSEC))

except KeyboardInterrupt:
    cv.destroyAllWindows()
