#

import requests

if __name__ == "__main__":
    resp = requests.post(
        "http://127.0.0.1:5000/weibo/download",
        json={
            "url": "https://weibo.com/2834272263/Pxsqugte",
        },
    )
    print(resp.status_code)
    print(resp.text)
