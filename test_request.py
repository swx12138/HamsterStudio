#

import requests

if __name__ == "__main__":
    resp = requests.post(
        "http://127.0.0.1:5000/weibo/download",
        json={
            "url": "https://weibo.com/5586261196/Px5KCC9aP",
        },
    )
    print(resp.status_code)
    print(resp.text)
