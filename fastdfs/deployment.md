# FastDfs 部署

## 环境
fs/fs1234
### Tracker
* Ubuntu 18.04.1 LTS
* 192.168.0.237

### Storage1
* Ubuntu 18.04.1 LTS
* 192.168.0.238

### Storage2
* Ubuntu 18.04.1 LTS
* 192.168.0.239

### 依赖

``` bash
sudo apt update
sudo apt install gcc make cmake unzip build-essential libtool libpcre3 libpcre3-dev zlib1g-dev

# 修改时区
sudo tzselect
sudo ln -sf /usr/share/zoneinfo/Asia/Shanghai /etc/localtime
```

## 安装

``` bash

# libfastcommon
git clone --depth=1 https://github.com/happyfish100/libfastcommon.git
cd libfastcommon
sudo ./make.sh
suod ./make.sh install

# fastdfs
git clone --depth=1 https://github.com/happyfish100/fastdfs.git fastdfs-5.12
cd fastdfs-5.12 && sudo ./make.sh && sudo ./make.sh install

```

## 配置

## Tracker
* 创建配置文件和根目录

``` bash
sudo cp /etc/fdfs/tracker.conf.sample /etc/fdfs/tracker.conf
sudo mkdir -p /fastdfs/tracker # tracker 的根目录
```
* 修改

``` bash
sudo vim /etc/fdfs/tracker.conf
base_path=/fastdfs/tracker  # 存储日志和数据的根目录
use_storage_id = true
storage_ids_filename = /etc/fdfs/storage_ids.conf
id_type_in_filename = id

```

* 防火墙打开22122端口
* 启动

``` bash
sudo /etc/init.d/fdfs_trackerd start
sudo /etc/init.d/fdfs_trackerd {start|stop|status|restart|condrestart} # 停止
```

* 开机启动

``` bash
# 直接使用systemd启动fdfs_trackerd会出错

# 配置systemd兼容在/etc/rc.local中设置开机启动程序
sudo ln -fs /lib/systemd/system/rc-local.service /etc/systemd/system/rc-local.service

sudo touch /etc/rc.local
sudo chmod 755 /etc/rc.local

cat > /etc/rc.local << EOF
#!/bin/bash

# start fdfs tracker
/etc/init.d/fdfs_trackerd start
EOF
```

### Storage

* 创建配置文件和根目录

``` bash
sudo cp /etc/fdfs/storage.conf.sample /etc/fdfs/storage.conf
sudo mkdir -p /fastdfs/storage # tracker 的根目录
```
* 修改

``` bash
sudo vim /etc/fdfs/storage.conf
group_name=group1                   # 组名（第一组为group1，第二组为group2，依次类推...）
base_path=/fastdfs/storage          # 数据和日志文件存储根目录
store_path0=/fastdfs/storage        # 第一个存储目录，第二个存储目录起名为：store_path1=xxx，其它存储目录名依次类推...
store_path_count=1                  # 存储路径个数，需要和store_path个数匹配
tracker_server=192.168.0.237:22122  # tracker服务器IP和端口
```

* 防火墙打开23000端口
* 启动

``` bash
sudo /etc/init.d/fdfs_storaged  start
sudo /etc/init.d/fdfs_storaged  stop # 停止
```
* 开机启动参照tracker

### Nginx

#### Storage

``` bash

# 下载nginx 和 fastdfs-nginx-module
cd ~/
wget http://nginx.org/download/nginx-1.14.0.tar.gz
git clone --depth=1 https://github.com/happyfish100/fastdfs-nginx-module.git

# 安装nginx
tar -zxvf nginx-1.14.0.tar.gz
cd nginx-1.14.0
./configure --prefix=/opt/nginx --sbin-path=/usr/bin/nginx --add-module=/home/fs/fastdfs-nginx-module/src
make && make install

# 配置
cp /home/fs/fastdfs-nginx-module/src/mod_fastdfs.conf /etc/fdfs/
vim /etc/fdfs/mod_fastdfs.conf

connect_timeout=10
base_path=/tmp
tracker_server=192.168.0.237:22122
storage_server_port=23000
group_name=group1                       # 第一组storage的组名
url_have_group_name=true
store_path0=/fastdfs/storage
group_count=1
[group1]
group_name=group1
storage_server_port=23000
store_path_count=1
store_path0=/fastdfs/storage

# 复制FastDFS源文件目录中HTTP相关的配置文件到/etc/fdfs目录
cd /home/fs/fastdfs-5.12/conf
cp http.conf mime.types /etc/fdfs/
# 创建数据存放目录的软链接
ln -s /fastdfs/storage/data/ /fastdfs/storage/data/M00

# 配置nginx
vim /opt/nginx/conf/nginx.conf

worker_processes 1;
events {
    worker_connections  1024;
}
http {
    include       mime.types;
    default_type  application/octet-stream;
    sendfile        on;
    keepalive_timeout  65;
    server {
        listen       8888;
        server_name  localhost;

        # FastDFS 文件访问配置(fastdfs-nginx-module模块)
        location ~/group([0-9])/M00 {
            ngx_fastdfs_module;
        }  
    }
}


# 启动nginx
nginx

# 开启启动

cat > /opt/nginx/nginx.service << EOF
[Unit]
Description=The NGINX HTTP and reverse proxy server
After=syslog.target network.target remote-fs.target nss-lookup.target

[Service]
Type=forking
PIDFile=/opt/nginx/logs/nginx.pid
ExecStartPre=/usr/bin/nginx -t
ExecStart=/usr/bin/nginx
ExecReload=/usr/bin/nginx -s reload
ExecStop=/bin/kill -s QUIT $MAINPID
PrivateTmp=true

[Install]
WantedBy=multi-user.target
EOF

ln -s /opt/nginx/nginx.service /etc/systemd/system/nginx.service
systemctl enable nginx.service
```

#### Tracker

``` bash
# 下载 ngx_cache_purge
wget -O ngx_cache_purge.zip https://github.com/FRiCKLE/ngx_cache_purge/archive/2.3.zip 
upzip ngx_cache_purge.zip

cd /home/fs/nginx-1.14.0
./configure --prefix=/opt/nginx --sbin-path=/usr/bin/nginx --add-module=/home/fs/ngx_cache_purge-2.3
make && make install

# 配置nginx
cat > /opt/nginx/conf/nginx.conf << EOF
#user  nobody;
worker_processes  1;
events {
    worker_connections  1024;
    use epoll;
}
http {
    include       mime.types;
    default_type  application/octet-stream;
    #log_format  main  '$remote_addr - $remote_user [$time_local] "$request" '
    #                  '$status $body_bytes_sent "$http_referer" '
    #                  '"$http_user_agent" "$http_x_forwarded_for"';
    #access_log  logs/access.log  main;
    sendfile       on;
    tcp_nopush     on;
    keepalive_timeout  65;
    #gzip on;

    #设置缓存
    server_names_hash_bucket_size 128;
    client_header_buffer_size 32k;
    large_client_header_buffers 4 32k;
    client_max_body_size 300m;
    proxy_redirect off;
    proxy_set_header Host $http_host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for; 
    proxy_connect_timeout 90;
    proxy_send_timeout 90;
    proxy_read_timeout 90;
    proxy_buffer_size 16k;
    proxy_buffers 4 64k;
    proxy_busy_buffers_size 128k;
    proxy_temp_file_write_size 128k; #设置缓存存储路径、存储方式、分配内存大小、磁盘最大空间、缓存期限 
    proxy_cache_path  /fastdfs/cache/nginx/proxy_cache/tmp keys_zone=tmpcache:10m;

    #设置 group1 的服务器
    upstream fdfs_group1 {
         server 192.168.0.238:8888 weight=1 max_fails=2 fail_timeout=30s;
         server 192.168.0.239:8888 weight=1 max_fails=2 fail_timeout=30s;
    }

    server {
        listen       80;
        server_name  localhost;
        #charset koi8-r;
        #access_log  logs/host.access.log  main;

        #设置 group 的负载均衡参数
        location /group1/M00 {
            proxy_next_upstream http_502 http_504 error timeout invalid_header;
            proxy_cache tmpcache;
            proxy_cache_valid  200 304 12h;
            proxy_cache_key $uri$is_args$args;
            proxy_pass http://fdfs_group1;
            expires 30d;
        }
		

        #设置清除缓存的访问权限
        location ~/purge(/.*) {
            allow 127.0.0.1;
            allow 192.168.0.0/24;
            deny all;
            proxy_cache_purge tmpcache $1$is_args$args;
        }
        #error_page  404              /404.html;
        # redirect server error pages to the static page /50x.html
        #
        error_page   500 502 503 504  /50x.html;
        location = /50x.html {
            root html; 
        }
    } 
}
EOF

# 创建对应的缓存目录：
mkdir -p /fastdfs/cache/nginx/proxy_cache/tmp
```
## 测试

* 在Tracker上
``` bash
 cp /etc/fdfs/client.conf.sample /etc/fdfs/client.conf
 vi /etc/fdfs/client.conf
base_path=/fastdfs/tracker
tracker_server=192.168.0.237:22122
# 上传
/usr/bin/fdfs_upload_file /etc/fdfs/client.conf /home/fs/1.txt
# 删除
/usr/bin/fdfs_delete_file /etc/fdfs/client.conf group1/M00/00/00/oYYBAFvZVs-AWTIMAAAABFqC_Qg122.txt
```