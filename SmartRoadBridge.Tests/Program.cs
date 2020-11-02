using SmartRoadBridge.Alignment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoadBridge.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Align M1K, L1K, R1K, M2K, M3K;
            Align SBE, EBA;
            Align JKA, MLA, SGRA;
            Align CCA, CCB, CCC, CCD;
            Align HSA, HSB, HSC, HSE, HSF;
            Align MUA, MUB, MUC;


            Align WLR;
            Align A8L2;
            string RootPart = @"G:\";
            M1K = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\00-MainLine\M1K-0312");
            L1K = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\00-MainLine\L1K-0312");
            R1K = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\00-MainLine\R1K-0312");
            M2K = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\00-MainLine\M2K-0312");
            M3K = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\00-MainLine\M3K-0312");
            MLA = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\08-Mlolongo Interchange\2+400-TQ");
            JKA = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\01-JKIA Interchange\JKIA-RAMP A");
            EBA = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\02-Eastern Bypass\A-TT");
            SBE = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\03-Southern Bypass\E");
            CCA = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\04-Capital Center\A4-4");
            CCB = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\04-Capital Center\B4-3");
            CCC = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\04-Capital Center\C4");
            CCD = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\04-Capital Center\D4");
            HSA = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\05-Haile Selassie\A");
            HSB = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\05-Haile Selassie\B");
            HSC = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\05-Haile Selassie\C");
            HSE = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\05-Haile Selassie\E");
            HSF = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\05-Haile Selassie\F");
            MUA = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\06-Museum Interchange\A匝道");
            MUB = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\06-Museum Interchange\B匝道");
            MUC = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\06-Museum Interchange\C匝道");
            WLR = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\07-Westlands Interchange\Westland-RampR");
            A8L2 = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\09-A8\A8L2");
            SGRA = new Align(RootPart + @"20191213-肯尼亚高架桥施工图设计(19406)\01 前方资料\EI Data\10-SGR Interchange\5900-OVERPASS");


            double f=MUB.GetGroundBGByCoord(469960.348, 9858648.054);
            
        }
    }
}
