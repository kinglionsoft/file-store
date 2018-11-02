# 分布式文件系统搭建
Service Fabric + FastDFS + AspNetCore 搭建分布式文件系统和存储服务。

## 安全性
* FastDFS以组为单位存储文件。每个组有多个存储节点（Storage)
* 上传、删除安全性：API 网关认证后可以访问
* 下载安全性：将FastDFS中特定的组定义为非安全组，存储可以公开访问的文件；其他组的文件，API 网关认证后可以访问

## 配置

## 使用
