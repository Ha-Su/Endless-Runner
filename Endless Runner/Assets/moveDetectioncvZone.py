import cv2
from cvzone.PoseModule import PoseDetector
import socket

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
serverAddressPort = ("127.0.0.1", 5054)

def check_shoulder_side(lmList, center_x):
    if not lmList or len(lmList) <= 12:
        return None

    # In cvzone PoseModule:
    # lmList[11] = left shoulder, lmList[12] = right shoulder
    left_sh_x  = lmList[11][0]
    right_sh_x = lmList[12][0]

    if left_sh_x  > center_x and right_sh_x > center_x:
        return "left"
    elif left_sh_x  < center_x and right_sh_x < center_x:
        return "right"
    else:
        return "center"

def main():
    # ---- CONFIGURATION ----
    FRAME_WIDTH   = 640
    FRAME_HEIGHT  = 480
    LINE_COLOR    = (0, 255, 0)  # BGR green for center line
    LINE_THICK    = 2
    CALIB_FRAMES  = 300          # number of frames to calibrate "standing" height
    JUMP_THRESH   = 35          # px above baseline → jump
    CROUCH_THRESH = 50          # px below baseline → crouch
    # -----------------------

    # Initialize PoseDetector
    detector = PoseDetector(
        staticMode=False,
        modelComplexity=1,
        smoothLandmarks=True,
        enableSegmentation=False,
        smoothSegmentation=True,
        detectionCon=0.5,
        trackCon=0.5
    )

    # Open webcam
    cap = cv2.VideoCapture(0)
    cap.set(cv2.CAP_PROP_FRAME_WIDTH,  FRAME_WIDTH)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, FRAME_HEIGHT)
    if not cap.isOpened():
        print("Could not open webcam")
        return

    # For calibration of standing-center‐y
    baseline_sum      = 0.0
    calib_count       = 0
    baseline_center_y = None

    while True:
        success, img = cap.read()
        if not success:
            print("❌ Failed to grab frame")
            break

        action = None
        side = None

        # 1) Pose detection & get landmarks + bbox
        img = detector.findPose(img)
        lmList, bboxInfo = detector.findPosition(
            img, draw=True, bboxWithHands=False
        )

        # 2) Draw vertical center line
        h, w     = img.shape[:2]
        center_x = w // 2
        cv2.line(img, (center_x, 0), (center_x, h), LINE_COLOR, LINE_THICK)

        # 3) Only calibrate/detect if full pose landmarks detected
        if lmList and len(lmList) == 33 and bboxInfo:
            cx, cy = bboxInfo["center"]  # center of bbox

            # -- Calibration phase --
            if baseline_center_y is None:
                if calib_count < CALIB_FRAMES:
                    baseline_sum += cy
                    calib_count += 1
                if calib_count >= CALIB_FRAMES:
                    baseline_center_y = baseline_sum / calib_count
                    print(f"Calibrated standing-center-y → {baseline_center_y:.1f}")
            else:
                # -- Action detection --
                if cy < baseline_center_y - JUMP_THRESH:
                    action = "jump"
                elif cy > baseline_center_y + CROUCH_THRESH:
                    action = "crouch"
                else:
                    action = "stand"

                # print & overlay
                print(action)
                cv2.putText(
                    img,
                    action.upper(),
                    (10, 80),
                    cv2.FONT_HERSHEY_SIMPLEX,
                    1,
                    (255, 0, 0),
                    2
                )

        # 4) Shoulder‐side detection (left/center/right)
        side = check_shoulder_side(lmList, center_x)
        if side:
            print(side)
            cv2.putText(
                img,
                side.upper(),
                (10, 40),
                cv2.FONT_HERSHEY_SIMPLEX,
                1,
                (0, 0, 255),
                2
            )

        if action and side:
            message = f"{action},{side}"
            print("Sending:", message)
            sock.sendto(message.encode(), serverAddressPort)

        # 5) Display
        cv2.imshow("Pose + Actions", img)

        # exit on 'q'
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

    # Cleanup
    cap.release()
    cv2.destroyAllWindows()

if __name__ == "__main__":
    main()
