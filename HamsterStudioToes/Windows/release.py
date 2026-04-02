# encoding utf-8

import os
import zipfile
import base64

zipn = "release.zip"


def encode():
    """创建压缩包"""
    with zipfile.ZipFile(zipn, "w", zipfile.ZIP_LZMA) as zip:
        for path, dirnames, filenames in os.walk("."):
            # print(path, dirnames, filenames)
            for filename in filenames:
                if filename.endswith((".cc", ".hh", ".py")) or filename.lower() == "makefile":
                    f = path + "\\" + filename
                    print("zipping", f, "...")
                    zip.write(f, f[2:])
    # 获取并返回压缩包的BASE64
    with open(zipn, "rb") as zipb:
        b64 = base64.b64encode(zipb.read())
        with open("release.txt", "w", encoding="utf-8") as zipt:
            zipt.write(b64.decode(encoding="utf-8"))


def decode():
    with open("release.txt", "r", encoding="utf-8") as zipt:
        txt = zipt.read()
        with open(zipn.replace(".zip", ".r.zip"), "wb") as zipb:
            b64 = base64.b64decode(txt)
            zipb.write(b64)


if __name__ == "__main__":
    encode()
    # decode()
