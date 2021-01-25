using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Debuff
{
    public delegate void DebuffHandler(int _id,float _deltatime);
    public DebuffHandler action;
    public float time;
    public Debuff()
    {

    }
    public Debuff(DebuffHandler _action)
    {
        action = _action;
    }
    public static void Burning(int _id,float _deltatime)
    {
        Server.clients[_id].player.Damage(2 * _deltatime);
    }
    
}


