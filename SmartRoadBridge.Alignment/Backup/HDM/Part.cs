using System.Collections.Generic;

namespace SmartRoadBridge.Alignment
{
    public enum TransitionEnum
    {
        线性变化 = 1,
        二次抛物线 = 2,
        三次抛物线 = 3
    }
    public struct JKB { double PK; double w; string desc; }


    public class Part
    {
        public Part(string name, int typeid, string w_express, string h_express, TransitionEnum w_transition, TransitionEnum h_transition, TransitionEnum v_transition, int colorIndex)
        {

            Name = name;
            Type = typeid;
            this.w_express = w_express;
            this.h_express = h_express;
            this.w_transition = w_transition;
            this.h_transition = h_transition;
            this.v_transition = v_transition;
            ColorIndex = colorIndex;
        }



        public string Name;
        int Type;
        string w_express, h_express;
        TransitionEnum w_transition, h_transition, v_transition;
        List<JKB> JKBCollection;
        int ColorIndex;



    }

}
