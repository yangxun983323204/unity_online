# coding:utf-8
import sys,getopt,os

if len(sys.argv)<=1:
    print("yx-gen.py -i <input_dir> -o <output_dir>")
    sys.exit(2)

try:
    opts,args = getopt.getopt(sys.argv[1:],"-h-i:-o:",[])
except getopt.GetoptError:
    print("yx-gen.py -i <input_dir> -o <output_dir>")
    sys.exit(2)

inputDir=''
outputDir=''

for opt,arg in opts:
    if opt == '-h':
        print("yx-gen.py -i <input_dir> -o <output_dir>")
        sys.exit()
    elif opt == '-i':
        inputDir=arg
    elif opt == '-o':
        outputDir=arg

print("\n==================代码生成开始==================")
print("输入目录为",inputDir)
print("输出目录为",outputDir)

for root,dirs,files in os.walk(inputDir,topdown=False):
    for file in files:
        if os.path.splitext(file)[-1]!=".proto":
            continue
        baseName = os.path.splitext(file)[0]
        src = os.path.join(inputDir,baseName)
        dst = os.path.join(outputDir,baseName)
        cmd = 'protogen -i:"{0}.proto" -o:"{1}.cs" -d'.format(src,dst).replace('\\','/')
        print("\n生成命令:",cmd,"\n")
        os.system(cmd)

print("\n==================代码生成完成==================")