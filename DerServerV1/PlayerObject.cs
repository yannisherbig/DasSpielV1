using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DerServer
{
    public class PlayerObject
    {
        private int id;
        private GameObject gobj;

        public Player(int id, GameObject gobj)
        {
            this.id = id;
            this.gobj = gobj;
        }
    }
}
