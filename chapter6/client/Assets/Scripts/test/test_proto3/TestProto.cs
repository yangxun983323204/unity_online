﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Google.Protobuf;

namespace Proto3Msg
{
    public class TestProto : MonoBehaviour
    {
        NetManager _netMgr = new NetManager();

        // Start is called before the first frame update
        void Start()
        {
            TestProtoSimple();
            _netMgr.Packer = new Proto3MsgPacker();
            _netMgr.NetEventDispatcher.AddListener(NetManager.NetEvent.ConnectSuccess, OnConnectSuccess);
            _netMgr.NetEventDispatcher.AddListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
            _netMgr.NetEventDispatcher.AddListener(NetManager.NetEvent.Close, OnClose);
            _netMgr.NetMsgDispatcher.AddListener(_netMgr.Packer.GetMsgName(typeof(MsgMove)), OnMsgMove);
            _netMgr.EnableHeartbeat(() => { return new MsgPing(); });
            _netMgr.Connect("127.0.0.1", 8888);
            StartCoroutine(RandMove());
        }
        void TestProtoSimple()
        {
            var msg = new MsgMove();
            msg.X = 4;
            msg.Y = -7;
            msg.Z = -4;
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                var ostream = new CodedOutputStream(ms);
                ostream.WriteMessage(msg as IMessage);
                ostream.Flush();
                bytes = ms.ToArray();
            }

            var istream = new CodedInputStream(bytes);
            var msg2 = System.Activator.CreateInstance(typeof(MsgMove)) as IMessage;
            istream.ReadMessage(msg2);

            Debug.LogFormat("msg:{0},msg2:{1}", msg, msg2);
        }
        // Update is called once per frame
        void Update()
        {
            _netMgr.Process();
        }

        IEnumerator RandMove()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(1, 3));
                var msg = new MsgMove();
                msg.X = Random.Range(-10, 10);
                msg.Y = Random.Range(-10, 10);
                msg.Z = Random.Range(-10, 10);
                Debug.LogFormat("Send:{0},{1},{2}", msg.X, msg.Y, msg.Z);
                _netMgr.Send(msg);
            }
        }

        void OnConnectSuccess(string msg)
        {
            Debug.Log("连接成功");
        }

        void OnConnectFail(string msg)
        {
            Debug.Log("连接失败" + msg);
        }

        void OnClose(string msg)
        {
            Debug.Log("关闭" + msg);
        }

        void OnMsgMove(object msg)
        {
            var move = msg as MsgMove;
            Debug.LogFormat("move:[{0},{1},{2}]", move.X.ToString(), move.Y.ToString(), move.Z.ToString());
        }

        private void OnDestroy()
        {
            _netMgr.Close();
        }
    }
}
