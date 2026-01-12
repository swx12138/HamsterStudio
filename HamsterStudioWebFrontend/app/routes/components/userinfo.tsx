import "./userinfo.css";

import type { IUserInfo } from "../interfaces/IMediaSiteInfo";
import { Avatar, Image, Layout } from "antd";
import { UserOutlined } from '@ant-design/icons';
import { getProxyImageUrl } from "../utils/util";

const { Header, Footer, Sider, Content } = Layout;

export default function UserInfo({ info }: Readonly<{ info: IUserInfo }>) {
    return (
        <div className="user-info-container">
            <Avatar className="user-info-avatar"
                size={64}
                icon={<Image
                    src={getProxyImageUrl(info.avatarUrl)}
                />}
            />
            <p className="user-info-name">{info.name}</p>
            <p className="user-info-description">{info.description}</p>
        </div>
    )
}
