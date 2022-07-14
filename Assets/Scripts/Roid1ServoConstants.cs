using System;
using System.Collections.Generic;
using UnityEngine;

namespace HumanoidTracer
{
    public class Roid1ServoConstants
    {
        /// <summary>
        /// 動作から除外する部位
        /// </summary>
        [Flags]
        public enum ServosMask
        {
            None = 0,
            UppeerBody = 1,
            LowerBody = 2,
        }
        
        /// <summary>
        /// サーボの取り付け位置
        /// </summary>
        public enum ServoPosition
        {
            Spine,               //体全体の基準
            
            HeadYaw,             //頭ヨー
            
            LeftShoulderPitch,   //肩ピッチL
            LeftShoulderRoll,    //肩ロールL
            LeftElbowYaw,        //肘ヨーL
            LeftLoweArmPitch,    //肘ピッチL

            LeftHipYaw,          //ヒップヨーL
            LeftHipRoll,         //ヒップロールL
            LeftUpperLegPitch,   //腿ピッチL
            LeftLowerLegPitch,   //肘ピッチL
            LeftFootPitch,       //足首ピッチL
            LeftFootRoll,        //足首ロールL

            ChestYaw,            //腰ヨー

            RightShoulderPitch,  //肩ピッチR
            RightShoulderRoll,   //肩ロールR
            RightElbowYaw,       //肘ヨーR
            RightLowerArmPitch,  //肘ピッチR

            RightHipYaw,         //ヒップヨーR
            RightHipRoll,        //ヒップロールR
            RightUpperLegPitch,  //腿ピッチR
            RightLowerLegPitch,  //膝ピッチR
            RightFootPitch,      //足首ピッチR
            RightFootRoll,       //足首ロールR
        }


        /// <summary>
        /// URDFでの名前との対応
        /// </summary>
        public static readonly Dictionary<ServoPosition, string> servoNames = new Dictionary<ServoPosition, string>()
        {
            {ServoPosition.Spine, "c_waist"},
            
            {ServoPosition.HeadYaw, "c_head"},
            
            {ServoPosition.LeftShoulderPitch, "l_shoulder"},
            {ServoPosition.LeftShoulderRoll, "l_upperarm"},
            {ServoPosition.LeftElbowYaw, "l_elbow"},
            {ServoPosition.LeftLoweArmPitch, "l_lowerarm"},
            
            {ServoPosition.LeftHipYaw, "l_hipjointupper"},
            {ServoPosition.LeftHipRoll, "l_hipjointlower"},
            {ServoPosition.LeftUpperLegPitch, "l_upperleg"},
            {ServoPosition.LeftLowerLegPitch, "l_lowerleg"},
            {ServoPosition.LeftFootPitch, "l_ankle"},
            {ServoPosition.LeftFootRoll, "l_foot"},
            
            {ServoPosition.ChestYaw, "c_chest"},
            
            {ServoPosition.RightShoulderPitch, "r_shoulder"},
            {ServoPosition.RightShoulderRoll, "r_upperarm"},
            {ServoPosition.RightElbowYaw, "r_elbow"},
            {ServoPosition.RightLowerArmPitch, "r_lowerarm"},
            
            {ServoPosition.RightHipYaw, "r_hipjointupper"},
            {ServoPosition.RightHipRoll, "r_hipjointlower"},
            {ServoPosition.RightUpperLegPitch, "r_upperleg"},
            {ServoPosition.RightLowerLegPitch, "r_lowerleg"},
            {ServoPosition.RightFootPitch, "r_ankle"},
            {ServoPosition.RightFootRoll, "r_foot"},
        };

        /// <summary>
        /// 回転を逆にする関節はfalse
        /// </summary>
        public static readonly Dictionary<ServoPosition, bool> servoDirections = new Dictionary<ServoPosition, bool>()
        {
            {ServoPosition.Spine, true},
            
            {ServoPosition.HeadYaw, false},

            {ServoPosition.LeftShoulderPitch, true},
            {ServoPosition.LeftShoulderRoll, false},
            {ServoPosition.LeftElbowYaw, true},
            {ServoPosition.LeftLoweArmPitch, true},

            {ServoPosition.LeftHipYaw, false},
            {ServoPosition.LeftHipRoll, false},
            {ServoPosition.LeftUpperLegPitch, true},
            {ServoPosition.LeftLowerLegPitch, true},
            {ServoPosition.LeftFootPitch, true},
            {ServoPosition.LeftFootRoll, false},
            
            {ServoPosition.ChestYaw, false},

            {ServoPosition.RightShoulderPitch, true},
            {ServoPosition.RightShoulderRoll, false},
            {ServoPosition.RightElbowYaw, true},
            {ServoPosition.RightLowerArmPitch, true},

            {ServoPosition.RightHipYaw, false},
            {ServoPosition.RightHipRoll, false},
            {ServoPosition.RightUpperLegPitch, true},
            {ServoPosition.RightLowerLegPitch, true},
            {ServoPosition.RightFootPitch, true},
            {ServoPosition.RightFootRoll, false},
        };

        /// <summary>
        /// 回転を逆にする関節はfalse
        /// </summary>
        public static readonly Dictionary<ServoPosition, float> servoOffsets = new Dictionary<ServoPosition, float>()
        {
            {ServoPosition.Spine, 0},

            {ServoPosition.HeadYaw, 0},

            {ServoPosition.LeftShoulderPitch, 0},
            {ServoPosition.LeftShoulderRoll, 90f},
            {ServoPosition.LeftElbowYaw, 0},
            {ServoPosition.LeftLoweArmPitch, 0},

            {ServoPosition.LeftHipYaw, 0},
            {ServoPosition.LeftHipRoll, 0},
            {ServoPosition.LeftUpperLegPitch, 0},
            {ServoPosition.LeftLowerLegPitch, 0},
            {ServoPosition.LeftFootPitch, 0},
            {ServoPosition.LeftFootRoll, 0},

            {ServoPosition.ChestYaw, 0},

            {ServoPosition.RightShoulderPitch, 0},
            {ServoPosition.RightShoulderRoll, -90f},
            {ServoPosition.RightElbowYaw, 0},
            {ServoPosition.RightLowerArmPitch, 0},

            {ServoPosition.RightHipYaw, 0},
            {ServoPosition.RightHipRoll, 0},
            {ServoPosition.RightUpperLegPitch, 0},
            {ServoPosition.RightLowerLegPitch, 0},
            {ServoPosition.RightFootPitch, 0},
            {ServoPosition.RightFootRoll, 0},
        };

        /// <summary>
        /// 全身の関節すべて
        /// </summary>
        public static readonly ServoPosition[] fullBodyServoPositions =
        {
            ServoPosition.HeadYaw,
            ServoPosition.ChestYaw,
            ServoPosition.RightShoulderPitch,
            ServoPosition.RightShoulderRoll,
            ServoPosition.RightElbowYaw,
            ServoPosition.RightLowerArmPitch,
            ServoPosition.LeftShoulderPitch,
            ServoPosition.LeftShoulderRoll,
            ServoPosition.LeftElbowYaw,
            ServoPosition.LeftLoweArmPitch,
            ServoPosition.RightHipYaw,
            ServoPosition.RightHipRoll,
            ServoPosition.RightUpperLegPitch,
            ServoPosition.RightLowerLegPitch,
            ServoPosition.RightFootPitch,
            ServoPosition.RightFootRoll,
            ServoPosition.LeftHipYaw,
            ServoPosition.LeftHipRoll,
            ServoPosition.LeftUpperLegPitch,
            ServoPosition.LeftLowerLegPitch,
            ServoPosition.LeftFootPitch,
            ServoPosition.LeftFootRoll,
        };

        /// <summary>
        /// 上半身の関節
        /// </summary>
        public static readonly ServoPosition[] upperBodyServoPositions =
        {
            ServoPosition.HeadYaw,
            ServoPosition.ChestYaw,
            ServoPosition.RightShoulderPitch,
            ServoPosition.RightShoulderRoll,
            ServoPosition.RightElbowYaw,
            ServoPosition.RightLowerArmPitch,
            ServoPosition.LeftShoulderPitch,
            ServoPosition.LeftShoulderRoll,
            ServoPosition.LeftElbowYaw,
            ServoPosition.LeftLoweArmPitch,
        };

        /// <summary>
        /// 下半身の関節すべて
        /// </summary>
        public static readonly ServoPosition[] lowerBodyServoPositions =
        {
            ServoPosition.RightHipYaw,
            ServoPosition.RightHipRoll,
            ServoPosition.RightUpperLegPitch,
            ServoPosition.RightLowerLegPitch,
            ServoPosition.RightFootPitch,
            ServoPosition.RightFootRoll,
            ServoPosition.LeftHipYaw,
            ServoPosition.LeftHipRoll,
            ServoPosition.LeftUpperLegPitch,
            ServoPosition.LeftLowerLegPitch,
            ServoPosition.LeftFootPitch,
            ServoPosition.LeftFootRoll,
        };
    }
}
