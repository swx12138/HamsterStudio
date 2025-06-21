#
import json


def read_json(filename: str):
    with open(filename, "r", encoding="utf-8") as f:
        return json.load(f)


def underline_to_big_camle(name: str):
    new_name = name[0].upper()
    try:
        idx = 1
        while idx < len(name) - 1:
            if name[idx] == "_":
                new_name += name[idx + 1].upper()
                idx = idx + 1
            else:
                new_name += name[idx]
            idx = idx + 1
    except Exception as ex:
        print(ex)
    return new_name


def to_camel_case(name: str):
    parts = map(lambda x: x[0].upper() + x[1:], name.split("_"))
    return "".join(parts)


class_metas = []


def parse_object(data: dict[str,], name: str):
    global class_metas

    class_meta = {"name": to_camel_case(name) + "Model", "members": []}
    if any(m["name"] == class_meta["name"] for m in class_metas):
        return class_meta

    if not isinstance(data, dict):
        return class_meta

    for k in data.keys():
        if isinstance(data[k], bool):
            class_meta["members"].append({"type": "bool", "name": k})
        elif isinstance(data[k], int):
            if data[k] >= 1 << 31:
                class_meta["members"].append({"type": "long", "name": k})
            else:
                class_meta["members"].append({"type": "int", "name": k})
        elif isinstance(data[k], float):
            class_meta["members"].append({"type": "double", "name": k})
        elif isinstance(data[k], str):
            class_meta["members"].append({"type": "string", "name": k})
        elif isinstance(data[k], list):
            if len(data[k]) == 0:
                class_meta["members"].append({"type": "object[]", "name": k})
            else:
                meta = parse_object(data[k][0], k + "Item")
                class_meta["members"].append({"type": meta["name"] + "[]", "name": k})
        elif isinstance(data[k], dict):
            meta = parse_object(data[k], k)
            class_meta["members"].append({"type": meta["name"], "name": k})
        elif data[k] == None:
            class_meta["members"].append({"type": "object?", "name": k})
        else:
            raise Exception("Not support.")

    class_metas.append(class_meta)
    return class_meta


def generate_mamber_code(member):
    name = member["name"]
    type: str = member["type"]
    json_property = f'\t[JsonPropertyName("{name}")]'
    declare = f"\tpublic {type} {to_camel_case(name)} {{get;set;}}"
    if type == "string":
        declare += "= string.Empty;"
    elif type.endswith("Model"):
        declare += "=new();"
    elif type.endswith("[]"):
        declare += "=[];"
    return json_property + "\n" + declare


def craete_csharp_class():
    global class_metas
    class_metas.reverse()
    for meta in class_metas:
        declare = f"public class {meta['name']}\n{{\n"
        member_lines = map(generate_mamber_code, meta["members"])
        yield declare + "\n\n".join(member_lines) + "\n}"


if __name__ == "__main__":
    json_data = read_json(r"D:\Code\HamsterStudio\Samples\weibo_show_Px74vs2RJ.json")
    class_meta = parse_object(json_data, "ShowData")
    # print(json.dumps(class_metas, indent=2, ensure_ascii=False))
    with open("DataModel.cs", "w") as fp:
        for cls in craete_csharp_class():
            print(cls, file=fp)
            print("", file=fp)
