Unity网络实战第二版代码练习

关于protobuf工具:
根目录下放了两个版本，protobuf-net和google protobuf

.proto文件生成cs:
在根目录protobuf-net/ProtoGen/和protobuf3/cshapr/中都提供了一个yx-gen.py用来批量生成文件夹下所有proto文件，但是protobuf-net只支持把proto文件放在它的目录之中（可以有子目录），google protobuf则任意位置都可以。