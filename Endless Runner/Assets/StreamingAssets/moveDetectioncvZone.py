import cv2
from cvzone.PoseModule import PoseDetector
import socket
import time

# ---- CONFIGURATION ----
FRAME_WIDTH        = 640
FRAME_HEIGHT       = 480

LINE_COLOR         = (0, 255, 0)
LINE_THICK         = 2

CALIB_HOLD_TIME    = 5.0    # secs to hold still for calibration
JUMP_THRESH        = 35      # px above baseline → jump

SIDE_MARGIN_RATIO  = 0.2    # 20% frame width for side detection
SIDE_HOLD_TIME     = 0.1     # secs to hold before confirming side

PRAY_THRESH        = 50      # px between wrists → praying
PRAY_DURATION      = 3.0     # secs to hold praying → startgame
# -----------------------

SIDE_MARGIN = int(FRAME_WIDTH * SIDE_MARGIN_RATIO)

# UDP setup
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
serverAddressPort = ("127.0.0.1", 5054)

# Pose detector
detector = PoseDetector(
    staticMode=False, modelComplexity=1,
    smoothLandmarks=True, enableSegmentation=False,
    smoothSegmentation=True, detectionCon=0.5, trackCon=0.5
)

cap = cv2.VideoCapture(0)
cap.set(cv2.CAP_PROP_FRAME_WIDTH,  FRAME_WIDTH)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, FRAME_HEIGHT)
if not cap.isOpened():
    print("Cannot open camera"); exit()

# State machine
calib_state          = "init"
calib_timer          = CALIB_HOLD_TIME
baseline_shoulder_y  = None

pray_timer           = 0.0
prevPray             = False

side_timer           = 0.0
confirmed_side       = "center"   # start in center
raw_side_prev        = "center"

last_time            = time.time()

def shoulder_avg_y(lmList):
    if not lmList or len(lmList) <= 12:
        return None
    return (lmList[11][1] + lmList[12][1]) / 2

def detect_raw_side(lmList):
    if not lmList or len(lmList) <= 12:
        return None
    lx, rx = lmList[11][0], lmList[12][0]
    cx = FRAME_WIDTH // 2
    if lx > cx + SIDE_MARGIN or rx > cx + SIDE_MARGIN:
        return "left"
    if lx < cx - SIDE_MARGIN or rx < cx - SIDE_MARGIN:
        return "right"
    return None

while True:
    success, img = cap.read()
    if not success:
        break

    now = time.time()
    dt  = now - last_time
    last_time = now

    img = detector.findPose(img)
    lmList, bboxInfo = detector.findPosition(img, draw=True, bboxWithHands=False)

    # ---- CALIBRATION ----
    if calib_state == "init":
        cv2.putText(img,
                    "Calibrating... stand up and go back a bit",
                    (20, FRAME_HEIGHT//2 - 20),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.8, (0,0,255), 2)
        if lmList and len(lmList) > 12:
            calib_state = "hold"

    elif calib_state == "hold":
        cv2.putText(img,
                    "Stand still for 10 seconds",
                    (20, FRAME_HEIGHT//2 - 20),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.8, (0,255,255), 2)
        cv2.putText(img,
                    f"{int(calib_timer)+1}",
                    (FRAME_WIDTH//2 - 20, FRAME_HEIGHT//2 + 40),
                    cv2.FONT_HERSHEY_SIMPLEX, 1.5, (0,255,255), 3)
        calib_timer -= dt
        if calib_timer <= 0 and lmList:
            baseline_shoulder_y = shoulder_avg_y(lmList)
            print(f"Calibrated baseline_shoulder_y = {baseline_shoulder_y:.1f}")
            calib_state = "calibrated"

    else:  # PLAY MODE
        action = None

        # draw baseline line
        y_line = int(baseline_shoulder_y)
        cv2.line(img, (0, y_line), (FRAME_WIDTH, y_line), LINE_COLOR, LINE_THICK)

        # STAND vs JUMP
        avg_y = shoulder_avg_y(lmList)
        if avg_y is not None:
            if avg_y < baseline_shoulder_y - JUMP_THRESH:
                action = "jump"
            else:
                action = "stand"
            cv2.putText(img,
                        action.upper(),
                        (10, 80),
                        cv2.FONT_HERSHEY_SIMPLEX, 1, (255,0,0), 2)

        # LEFT/RIGHT raw detection + hold-to-confirm
        raw_side = detect_raw_side(lmList) or "center"
        if raw_side == raw_side_prev:
            side_timer += dt
        else:
            side_timer = 0.0
        raw_side_prev = raw_side

        if side_timer >= SIDE_HOLD_TIME:
            if confirmed_side != raw_side:
                confirmed_side = raw_side
                print("Confirmed side:", confirmed_side)
        elif raw_side == "center":
            confirmed_side = "center"

        cv2.putText(img,
                    confirmed_side.upper(),
                    (10, 40),
                    cv2.FONT_HERSHEY_SIMPLEX, 1, (0,0,255), 2)

        # PRAYING gesture
        if lmList and len(lmList) == 33:
            lx, ly = lmList[15][0], lmList[15][1]
            rx, ry = lmList[16][0], lmList[16][1]
            dist   = ((lx-rx)**2 + (ly-ry)**2)**0.5

            if dist < PRAY_THRESH:
                if not prevPray:
                    sock.sendto("praying,none".encode(), serverAddressPort)
                    print("Sending: praying,none")
                    prevPray = True
                pray_timer += dt
                cv2.putText(img,
                            "PRAYING",
                            (10, 120),
                            cv2.FONT_HERSHEY_SIMPLEX, 1, (0,255,255), 2)
                if pray_timer >= PRAY_DURATION:
                    sock.sendto("startgame,none".encode(), serverAddressPort)
                    print("Sending: startgame,none")
                    pray_timer = 0.0
            else:
                prevPray   = False
                pray_timer = 0.0

        # SEND action + confirmed_side
        if action and confirmed_side:
            msg = f"{action},{confirmed_side}"
            sock.sendto(msg.encode(), serverAddressPort)
            print("Sending:", msg)

    cv2.imshow("Calibration & Play", img)
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap
