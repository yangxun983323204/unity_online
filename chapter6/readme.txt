客户端框架
可使用chapter2\async\async_echo_server生成的exe作为服务端测试

分别实现了三种消息格式：
基于unity json库
基于google protobuf proto3
基于protobuf-net proto2

本来只准备实现基于proto-net的proto2格式，但是由于测试服务器有个bug，把字节转了字符串再转成字节，导致二进制格式的消息返回后内容改变，一直以为是protobuf-net库的问题，所以又用google的protobuf做了一遍，仍然有问题，对比发送和收到的字节才发现问题...


.proto文件生成cs:
在根目录protobuf-net/ProtoGen/和protobuf3/cshapr/中都提供了一个yx-gen.py用来批量生成文件夹下所有proto文件，但是protobuf-net只支持把proto文件放在它的目录之中（可以有子目录），google protobuf则任意位置都可以。