import cv2
import numpy as np
from sklearn.cluster import DBSCAN


def get_bboxes(image):
        # GaussianBlur to reduce image noise
        blurred_image = cv2.GaussianBlur(image, (5, 5), 1.4)

        # Apply Canny edge detection
        edges = cv2.Canny(blurred_image, 100, 200)

        # Find coordinates of edge points
        y, x = np.where(edges > 0)
        edge_points = np.column_stack((x, y))

        # Apply DBSCAN clustering based on distance
        db = DBSCAN(eps=10, min_samples=5).fit(edge_points)
        labels = db.labels_

        # Get the number of clusters
        n_clusters = len(set(labels)) - (1 if -1 in labels else 0)

        bboxes = []
        for label in set(labels):
                if label == -1:
                        continue
                # Get the coordinates of the points in the cluster
                cluster_points = edge_points[labels == label]
                # Compute the bounding box
                x_min, y_min = np.min(cluster_points, axis=0)
                x_max, y_max = np.max(cluster_points, axis=0)
                bboxes.append(x_min)
                bboxes.append(y_min)
                bboxes.append(x_max)
                bboxes.append(y_max)
        return bboxes
