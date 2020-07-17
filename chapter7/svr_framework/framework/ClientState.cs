using System;
using System.Collections.Generic;
using System.Net.Sockets;

public class ClientState{

    public Socket Sock;
    public ByteArray RecBuffer = new ByteArray();

    public WriteQueue SendQueue = new WriteQueue();

    public long LastMsgTime = 0;
}