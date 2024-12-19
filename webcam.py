import cv2

from pyzbar.pyzbar import decode
cap = cv2.VideoCapture(0)
cap.set(3, 640)
cap.set(4, 480)

scanned_barcodes = set()  # Taratılan barkodları saklamak için bir set kullanıyoruz
camera = True
while camera:
    success, frame = cap.read()
    if not success:
        break
    for barcode in decode(frame):
        barcode_data = barcode.data.decode('utf-8')
        # print(barcode_data)

        if barcode_data not in scanned_barcodes:
            scanned_barcodes.add(barcode_data)
            print(f"Type: {barcode.type} | Data: {barcode_data}")

            # Barkod çevresi için dikdörtgen çizin
            points = barcode.polygon
            if len(points) == 4:
                pts = [(point.x, point.y) for point in points]
                cv2.polylines(frame, [np.array(pts, np.int32)], True, (0, 255, 0), 3)

            # Barkod verisini görüntüye yazın
            cv2.putText(frame, f'{barcode_data}', (barcode.rect.left, barcode.rect.top), cv2.FONT_HERSHEY_SIMPLEX, 0.9, (255, 0, 0), 2)
    cv2.imshow('Testing-code-scan', frame)
    print(scanned_barcodes)
    if cv2.waitKey(1) & 0xFF == ord('q'):
        camera = False

cap.release()
cv2.destroyAllWindows()
