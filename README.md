<font size=4> README: 中文 | <a href="./README-en.md">English</a>  </font>

# Custom Installer
C#实现的自定义安装程序。

最初是给自己安装小工具用的，因为有小伙伴需要源码，这里分享一下。





### 主要实现了以下基本安装包功能
* 释放文件
* 安装依赖
* 注册COM组件
* 创建桌面快捷方式/开机启动
* 创建控制面板卸载程序项
* 安装进度及状态显示



### 安装演示



https://github.com/zhaotianff/CustomInstaller/assets/22126367/3aad6699-b526-4215-bc25-5419e4a38ab9



### 卸载演示



https://github.com/zhaotianff/CustomInstaller/assets/22126367/683c3b6d-a5d4-4405-9fd3-e2029e307737



详细的介绍可以查看博客园

https://www.cnblogs.com/zhaotianff/p/17387496.html

> 代码还有可以优化的地方，我自己估计暂时不会去做这个工作，有需要的小伙伴可以自己优化  
> 需要区分x86和x64的系统，目前默认是AnyCPU，在某些系统上运行可能会出错，这里需要注意下。



### LICENSE
[MIT](LICENSE)
