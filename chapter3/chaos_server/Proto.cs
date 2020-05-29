using System.Collections.Generic;
using System.Text;

namespace chaos_server
{
    static class Proto
    {
        public static void SendList(ClientState client)
        {
            StringBuilder sb = new StringBuilder(40);
            sb.Append("List|");
            foreach(var state in Program._clients.Values)
            {
                sb.Append($"{state.id},{state.x},{state.y},{state.z},{state.yEuler}&");
            }

            sb.Remove(sb.Length-1,1);
            Program.Send(client,sb.ToString());
        }
    }
}