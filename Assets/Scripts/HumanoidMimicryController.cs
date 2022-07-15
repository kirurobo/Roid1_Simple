// 指定したHumanoidの動作をロボットに真似させます
//
// 使い方
//  1. ロボットモデルにこのスクリプトをアタッチしておく
//  2. 動作元となるモデルを sourceAnimator に指定
//
// 制限事項
//  - 動作元となるモデルは Humanoid で、必要なボーンが揃っていること
//  - 動作元となるモデルは Start() 時点では Tポーズ をとっていること
//  - 反映先ロボットは Roid1 の構成を前提としている
//  - 反映先ロボットは Start() の時点ではTポーズから肩を90度下におろした姿勢とする

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using static HumanoidTracer.Roid1ServoConstants;

namespace HumanoidTracer
{
    public class HumanoidMimicryController : MonoBehaviour
    {
        public enum LR
        {
            Left,
            Right
        }

        public enum Axis
        {
            X,
            Y,
            Z
        }

        class Servo
        {
            public LR lr;
            public Axis axis;
            public float angle;
            public float offset;        // Tポーズとの差異

            public Quaternion rotation { get; private set; }
            
            private Servo parent;

            public ArticulationBody body { get; private set; }

            /// <summary>
            /// angleの回転方向を逆にしなければならない場合はtrueを返す
            /// </summary>
            public bool isInverse { get; private set; }
            
            /// <summary>
            /// サーボの角度を指定
            /// </summary>
            /// <param name="theta">指定角度</param>
            /// <returns>制限を超えた場合は制限までに直した角度</returns>
            public float SetServoValue(float theta)
            {
                angle = theta;
                
                if (body != null)
                {
                    var xDrive = body.xDrive;
                    float targetAngle = theta + offset;

                    // 制限を超えた場合は制限までに直す
                    if (xDrive.lowerLimit <= xDrive.upperLimit)
                    {
                        targetAngle = Mathf.Clamp(targetAngle, xDrive.lowerLimit, xDrive.upperLimit);
                    }
                    else
                    {
                        targetAngle = Mathf.Clamp(targetAngle, xDrive.upperLimit, xDrive.lowerLimit);
                    }
                    angle = targetAngle - offset;

                    // 目標角度を設定
                    xDrive.target = targetAngle;
                    body.xDrive = xDrive;
                }
                return angle;
            }
            

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="body"></param>
            public Servo(ArticulationBody body, bool direction = true, float offset = 0)
            {
                this.angle = 0f;
                this.lr = LR.Left;
                this.axis = Axis.X;

                this.offset = offset;
                this.body = body;

                if (body != null)
                {
                    var xDrive = body.xDrive;

                    this.isInverse = !direction;

                    // Anchor Rotation で軸を判断
                    var eular = body.anchorRotation.eulerAngles;
                    if (Mathf.Abs(eular.z) > 80f)
                    {
                        this.axis = Axis.Y;    // ヨーと判断
                    }
                    else if (Mathf.Abs(eular.y) > 80f)
                    {
                        this.axis = Axis.Z;    // ロールと判断
                    }
                    else
                    {
                        this.axis = Axis.X;    // ピッチと判断
                    }
                    //Debug.Log(body.name + " : " + axis);
                }
            }
        }

        /// <summary>
        /// 動作元モデルとするHumanoid
        /// </summary>
        [Tooltip("このモデルの動作を真似します。省略すると自分のAnimatorを使います")]
        public Animator sourceHumanoid;

        [Tooltip("ここで指定した部位は動かしません")]
        public ServosMask servosMask = ServosMask.None;

        /// <summary>
        /// 反映先ロボットモデル
        /// </summary>
        [Tooltip("反映先ロボットモデルです。それ自体にアタッチされていれば未指定で構いません")]
        private Transform robotRoot;

        private Dictionary<ServoPosition, Servo> servos = new Dictionary<ServoPosition, Servo>();

        /// <summary>
        /// 動作元モデルの必要関節初期姿勢
        /// </summary>
        private Dictionary<HumanBodyBones, Quaternion> invOrgRotations = new Dictionary<HumanBodyBones, Quaternion>();

        // 上半身、下半身それぞれの根本となるボーン（モデルによってはChestが無かったりするため変数としておく）
        private HumanBodyBones upperBodyRootBone = HumanBodyBones.Chest;
        private HumanBodyBones lowerBodyRootBone = HumanBodyBones.Hips;


        /// <summary>
        /// 姿勢の反映先部位の定義
        /// Roid1 用
        /// </summary>
        void SetupRobotBodies()
        {
            servos.Clear();
            
            if (robotRoot == null) return;

            foreach (var bone in servoNames)
            {
                var servoPosition = bone.Key;
                var name = bone.Value;
                var direction = servoDirections[servoPosition];
                var offset = servoOffsets[servoPosition];
                var tr = findRecursive(robotRoot, name);
                var body = tr?.GetComponent<ArticulationBody>();
                Servo servo = new Servo(body, direction, offset);
                servos.Add(servoPosition, servo);
            }
        }
        
        /// <summary>
        /// 再帰的に指定名称のTransformを探す
        /// </summary>
        /// <param name="root"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        Transform findRecursive(Transform root, string name)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name == name)
                {
                    return child;
                }
                var obj = findRecursive(child, name);
                if (obj != null)
                {
                    return obj;
                }
            }
            return null;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (!robotRoot)
            {
                // 未指定ならモデルのルートにこのスクリプトがアタッチされているものとする
                robotRoot = transform;
            }

            // 元となるHumanoidが未指定なら、自分自身のAnimatorを真似する
            if (!sourceHumanoid)
            {
                sourceHumanoid = robotRoot.GetComponent<Animator>();
            }

            // モーション反映先部位を定義
            SetupRobotBodies();

            // 動作元モデルの初期姿勢を記憶
            InitializeOriginalRotations();
        }

        /// <summary>
        /// 姿勢を各サーボの角度に反映
        /// </summary>
        void LateUpdate()
        {
            if (!sourceHumanoid) return;

            if ((servosMask & ServosMask.UppeerBody) == ServosMask.None)
            {
                UpdateChest();
                UpdateHead();
                UpdateRightArm();
                UpdateLeftArm();
            }

            if ((servosMask & ServosMask.LowerBody) == ServosMask.None)
            {
                UpdateRightLeg();
                UpdateLeftLeg();
            }
        }
        
        /// <summary>
        /// 動作元モデル各部の、親に対する初期姿勢を記憶
        /// Tポーズであることを前提とする
        /// </summary>
        void InitializeOriginalRotations()
        {
            invOrgRotations.Clear();

            if (!sourceHumanoid) return;

            Quaternion invRootRot, invParentRot, invRot;
            Transform parentTransform;

            invRootRot = Quaternion.Inverse(sourceHumanoid.transform.rotation);

            // 上半身の基部
            upperBodyRootBone = HumanBodyBones.Chest;
            parentTransform = sourceHumanoid.GetBoneTransform(upperBodyRootBone);
            if (!parentTransform)
            {
                upperBodyRootBone = HumanBodyBones.Spine;
                parentTransform = sourceHumanoid.GetBoneTransform(upperBodyRootBone);
            }
            if (!parentTransform)
            {
                upperBodyRootBone = HumanBodyBones.Hips;
                parentTransform = sourceHumanoid.GetBoneTransform(upperBodyRootBone);
            }
            AddInvOriginalRotation(upperBodyRootBone, invRootRot);
            invParentRot = Quaternion.Inverse(parentTransform.rotation);

            // 頭部
            AddInvOriginalRotation(HumanBodyBones.Head, invParentRot);

            // 右腕部
            invRot = AddInvOriginalRotation(HumanBodyBones.RightUpperArm, invParentRot);
            invRot = AddInvOriginalRotation(HumanBodyBones.RightLowerArm, invParentRot);
            invRot = AddInvOriginalRotation(HumanBodyBones.RightHand, invParentRot);

            // 左腕部
            invRot = AddInvOriginalRotation(HumanBodyBones.LeftUpperArm, invParentRot);
            invRot = AddInvOriginalRotation(HumanBodyBones.LeftLowerArm, invParentRot);
            invRot = AddInvOriginalRotation(HumanBodyBones.LeftHand, invParentRot);

            // 下半身の基部
            lowerBodyRootBone = HumanBodyBones.Spine;
            parentTransform = sourceHumanoid.GetBoneTransform(lowerBodyRootBone);
            invParentRot = Quaternion.Inverse(parentTransform.rotation);
            if (upperBodyRootBone != lowerBodyRootBone)
            {
                AddInvOriginalRotation(lowerBodyRootBone, invRootRot);
            }

            // 右脚部
            invRot = AddInvOriginalRotation(HumanBodyBones.RightUpperLeg, invParentRot);
            invRot = AddInvOriginalRotation(HumanBodyBones.RightLowerLeg, invParentRot);
            invRot = AddInvOriginalRotation(HumanBodyBones.RightFoot, invParentRot);

            // 左脚部
            invRot = AddInvOriginalRotation(HumanBodyBones.LeftUpperLeg, invParentRot);
            invRot = AddInvOriginalRotation(HumanBodyBones.LeftLowerLeg, invParentRot);
            invRot = AddInvOriginalRotation(HumanBodyBones.LeftFoot, invParentRot);
        }

        /// <summary>
        /// 動作元ボーン一つ分を記憶
        /// </summary>
        /// <param name="servoNo"></param>
        Quaternion AddInvOriginalRotation(HumanBodyBones bone, Quaternion invParentRotation)
        {
            Transform tr = sourceHumanoid.GetBoneTransform(bone);
            Quaternion rot = invParentRotation * tr.rotation;
            Quaternion invRot = Quaternion.Inverse(rot);
            invOrgRotations.Add(bone, invRot);
            return Quaternion.Inverse(tr.rotation);
        }

        /// <summary>
        /// Quaternion で渡された姿勢のうち、X, Y, Z 軸いずれか周り成分を抽出してサーボ角に反映します
        /// </summary>
        /// <param name="rot">目標姿勢</param>
        /// <param name="joint">指定サーボ</param>
        /// <returns>回転させた軸成分を除いた残りの回転 Quaternion</returns>
        Quaternion ApplyPartialRotation(Quaternion rot, Servo joint)
        {
            Quaternion q = rot;
            Vector3 axis = Vector3.right;
            float direction = (joint.isInverse ? -1f : 1f);     // 逆転なら-1
            switch (joint.axis)
            {
                case Axis.X:
                    q.y = q.z = 0;
                    if (q.x < 0) direction = -direction;
                    axis = Vector3.right;
                    break;
                case Axis.Y:
                    q.x = q.z = 0;
                    if (q.y < 0) direction = -direction;
                    axis = Vector3.up;
                    break;
                case Axis.Z:
                    q.x = q.y = 0;
                    if (q.z < 0) direction = -direction;
                    axis = Vector3.forward;
                    break;
            }
            if (q.w == 0 && q.x == 0 && q.y == 0 && q.z == 0)
            {
                //Debug.Log("Joint: " + joint.name + " rotation N/A");
                q = Quaternion.identity;
            }
            q.Normalize();
            float angle = Mathf.Acos(q.w) * 2.0f * Mathf.Rad2Deg * direction;

            var actualAngle = joint.SetServoValue(angle);
            //Debug.Log(actualAngle);

            return rot * Quaternion.Inverse(q);
        }

        /// <summary>
        /// Quaternion で渡された姿勢のうち、X, Y, Z 軸いずれか周り成分を抽出してサーボ角に反映します
        /// </summary>
        /// <param name="rot">目標姿勢</param>
        /// <param name="joint">指定サーボ</param>
        /// <returns>回転させた軸成分を除いた残りの回転 Quaternion</returns>
        Quaternion ApplyPartialRotation(Quaternion rot, Quaternion invRot, Servo joint)
        {
            Quaternion q = rot;
            Vector3 axis = Vector3.right;
            float direction = (joint.isInverse ? -1f : 1f);     // 逆転なら-1
            switch (joint.axis)
            {
                case Axis.X:
                    q.y = q.z = 0;
                    if (q.x < 0) direction = -direction;
                    axis = Vector3.right;
                    break;
                case Axis.Y:
                    q.x = q.z = 0;
                    if (q.y < 0) direction = -direction;
                    axis = Vector3.up;
                    break;
                case Axis.Z:
                    q.x = q.y = 0;
                    if (q.z < 0) direction = -direction;
                    axis = Vector3.forward;
                    break;
            }
            if (q.w == 0 && q.x == 0 && q.y == 0 && q.z == 0)
            {
                //Debug.Log("Joint: " + joint.name + " rotation N/A");
                q = Quaternion.identity;
            }
            q.Normalize();
            float angle = Mathf.Acos(q.w) * 2.0f * Mathf.Rad2Deg * direction;

            var actualAngle = joint.SetServoValue(angle);

            return Quaternion.Inverse(Quaternion.AngleAxis(actualAngle / direction, axis)) * invRot;
        }

        /// <summary>
        /// Quaternion で渡された姿勢のうち、X, Y, Z 軸いずれか周り成分を抽出してサーボ角に反映します
        /// </summary>
        /// <param name="rot">目標姿勢</param>
        /// <param name="joint">指定サーボ</param>
        /// <returns>回転させた軸成分を除いた残りの回転 Quaternion</returns>
        Quaternion ApplyDirectRotation(Quaternion rot, float angle, Servo joint)
        {
            Quaternion q;
            Vector3 axis = Vector3.right;
            float direction = (joint.isInverse ? -1f : 1f);     // 逆転なら-1
            switch (joint.axis)
            {
                case Axis.X:
                    axis = Vector3.right;
                    break;
                case Axis.Y:
                    axis = Vector3.up;
                    break;
                case Axis.Z:
                    axis = Vector3.forward;
                    break;
            }
            var actualAngle = joint.SetServoValue(angle * direction);

            q = Quaternion.AngleAxis(angle, axis);

            q = Quaternion.AngleAxis(actualAngle / direction, axis);    // 角度制限を考慮
            return Quaternion.Inverse(q) * rot;
        }

        /// <summary>
        /// 指定角度がほぼゼロならばtrue
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        bool IsAngleApproximatelyZero(float angle)
        {
            const float threshold = 1.0f;   // これ未満ならほぼ0と見做す角度[deg]
            return (Mathf.Abs(angle) < threshold);
        }

        
        /// <summary>
        /// 頭部の姿勢を反映
        /// </summary>
        void UpdateHead()
        {
            HumanBodyBones bone;    // 対象ボーン
            Transform tr;           // 対象Transformの作業用変数
            Quaternion rot;         // 相対姿勢の作業用変数

            // 頭部の親
            Transform parentTransform = sourceHumanoid.GetBoneTransform(upperBodyRootBone);
            Quaternion invParentRot = Quaternion.Inverse(parentTransform.rotation);

            // 頭部姿勢を反映
            bone = HumanBodyBones.Head;
            tr = sourceHumanoid.GetBoneTransform(bone);
            rot = invOrgRotations[bone] * invParentRot * tr.rotation;
            rot = ApplyPartialRotation(rot, servos[ServoPosition.HeadYaw]);

            //Debug.Log("Rot: " + tr.rotation + " Name: " + tr.name);
            //Debug.Log(servos[ServoPosition.HeadYaw]);
        }

        /// <summary>
        /// 胸部の姿勢を反映
        /// </summary>
        void UpdateChest()
        {
            HumanBodyBones bone;    // 対象ボーン
            Transform tr;           // 対象Transformの作業用変数
            Quaternion rot;         // 相対姿勢の作業用変数

            // 下半身を親とする
            Transform parentTransform = sourceHumanoid.GetBoneTransform(lowerBodyRootBone);
            Quaternion invParentRot = Quaternion.Inverse(parentTransform.rotation);

            // 胸部姿勢を反映
            bone = upperBodyRootBone;
            tr = sourceHumanoid.GetBoneTransform(upperBodyRootBone);
            rot = invOrgRotations[bone] * invParentRot * tr.rotation;
            rot = ApplyPartialRotation(rot, servos[ServoPosition.ChestYaw]);
        }

        /// <summary>
        /// 右肩以降の姿勢を反映
        /// </summary>
        void UpdateRightArm()
        {
            // 使いまわす変数
            HumanBodyBones bone;    // 対象ボーン
            Transform tr;           // 対象Transformの作業用変数
            Quaternion rot;         // 相対姿勢の作業用変数
            Vector3 euler;          // オイラー角で姿勢を表す際の変数
            Vector3 xt;             // 姿勢方向ベクトル
            Quaternion invRot;      // ワールド座標系の姿勢に直すための逆回転クォータニオン

            // 両腕の親
            Transform parentTransform = sourceHumanoid.GetBoneTransform(upperBodyRootBone);
            Quaternion invParentRot = Quaternion.Inverse(parentTransform.rotation);

            // 右上腕姿勢（ひねりは無視した長軸の方向）を反映
            bone = HumanBodyBones.RightUpperArm;
            tr = sourceHumanoid.GetBoneTransform(bone);
            rot = invParentRot * tr.rotation * invOrgRotations[bone];
            xt = rot * Vector3.right;
            euler.x = -Mathf.Atan2(xt.z, -xt.y) * Mathf.Rad2Deg;
            euler.z = -Mathf.Atan2(Mathf.Sqrt(xt.y * xt.y + xt.z * xt.z), xt.x) * Mathf.Rad2Deg;
            euler.y = 0f;
            if (IsAngleApproximatelyZero(euler.z))
            {
                euler.x = servos[ServoPosition.RightShoulderPitch].angle;    // 上腕が真横に伸びた状態ならば、肩は今の姿勢をキープ
            }
            invRot = Quaternion.identity;
            invRot = ApplyDirectRotation(invRot, euler.x, servos[ServoPosition.RightShoulderPitch]);
            invRot = ApplyDirectRotation(invRot, euler.z, servos[ServoPosition.RightShoulderRoll]);

            // 右前腕姿勢（ひねりは無視した長軸の方向）を反映
            bone = HumanBodyBones.RightLowerArm;
            tr = sourceHumanoid.GetBoneTransform(bone);
            rot = invRot * invParentRot * tr.rotation * invOrgRotations[bone];
            xt = rot * Vector3.right;
            euler.x = -Mathf.Atan2(xt.y, xt.z) * Mathf.Rad2Deg;
            euler.y = -Mathf.Atan2(Mathf.Sqrt(xt.y * xt.y + xt.z * xt.z), xt.x) * Mathf.Rad2Deg;
            euler.z = 0f;
            if (IsAngleApproximatelyZero(euler.y))
            {
                euler.x = servos[ServoPosition.RightElbowYaw].angle;    // 前腕が伸びた状態ならば、肘は今の姿勢をキープ
            }
            invRot = ApplyDirectRotation(invRot, euler.x, servos[ServoPosition.RightElbowYaw]);
            invRot = ApplyDirectRotation(invRot, euler.y, servos[ServoPosition.RightLowerArmPitch]);
        }

        /// <summary>
        /// 左肩以降の姿勢を反映
        /// </summary>
        void UpdateLeftArm()
        {
            // 使いまわす変数
            HumanBodyBones bone;    // 対象ボーン
            Transform tr;           // 対象Transformの作業用変数
            Quaternion rot;         // 相対姿勢の作業用変数
            Vector3 euler;          // オイラー角で姿勢を表す際の変数
            Vector3 xt;             // 姿勢方向ベクトル
            Quaternion invRot;      // ワールド座標系の姿勢に直すための逆回転クォータニオン

            // 両腕の親
            Transform parentTransform = sourceHumanoid.GetBoneTransform(upperBodyRootBone);
            Quaternion invParentRot = Quaternion.Inverse(parentTransform.rotation);

            // 左上腕姿勢を反映
            bone = HumanBodyBones.LeftUpperArm;
            tr = sourceHumanoid.GetBoneTransform(bone);
            rot = invParentRot * tr.rotation * invOrgRotations[bone];
            xt = rot * Vector3.right;
            euler.x = Mathf.Atan2(xt.z, xt.y) * Mathf.Rad2Deg;
            euler.z = Mathf.Atan2(Mathf.Sqrt(xt.y * xt.y + xt.z * xt.z), xt.x) * Mathf.Rad2Deg;
            euler.y = 0f;
            //Debug.Log("Rot: " + rot + "  TmpPos: " + xt + " Euler: " + euler);
            if (IsAngleApproximatelyZero(euler.z))
            {
                euler.x = servos[ServoPosition.LeftShoulderPitch].angle;    // 上腕が真横に伸びた状態ならば、肩は今の姿勢をキープ
            }
            invRot = Quaternion.identity;
            invRot = ApplyDirectRotation(invRot, euler.x, servos[ServoPosition.LeftShoulderPitch]);
            invRot = ApplyDirectRotation(invRot, euler.z, servos[ServoPosition.LeftShoulderRoll]);

            // 左前腕姿勢を反映
            bone = HumanBodyBones.LeftLowerArm;
            tr = sourceHumanoid.GetBoneTransform(bone);
            rot = invRot * invParentRot * tr.rotation * invOrgRotations[bone];
            xt = rot * Vector3.right;
            euler.x = Mathf.Atan2(-xt.y, -xt.z) * Mathf.Rad2Deg;
            euler.y = -Mathf.Atan2(Mathf.Sqrt(xt.y * xt.y + xt.z * xt.z), xt.x) * Mathf.Rad2Deg;
            euler.z = 0f;
            if (IsAngleApproximatelyZero(euler.y))
            {
                euler.x = servos[ServoPosition.LeftElbowYaw].angle;    // 前腕が伸びた状態ならば、肘は今の姿勢をキープ
            }
            invRot = ApplyDirectRotation(invRot, euler.x, servos[ServoPosition.LeftElbowYaw]);
            invRot = ApplyDirectRotation(invRot, euler.y, servos[ServoPosition.LeftLoweArmPitch]);
        }

        /// <summary>
        /// 右脚以降の姿勢を反映
        /// </summary>
        void UpdateRightLeg()
        {
            // 使いまわす変数
            HumanBodyBones bone;    // 対象ボーン
            Transform tr;           // 対象Transformの作業用変数
            Quaternion rot;         // 相対姿勢の作業用変数
            Vector3 euler;          // オイラー角で姿勢を表す際の変数
            Quaternion invRot;      // ワールド座標系の姿勢に直すための逆回転クォータニオン

            // 両脚の親
            Transform parentTransform = sourceHumanoid.GetBoneTransform(lowerBodyRootBone);
            Quaternion invParentRot = Quaternion.Inverse(parentTransform.rotation);

            // 右大腿姿勢を反映
            bone = HumanBodyBones.RightUpperLeg;
            tr = sourceHumanoid.GetBoneTransform(bone);
            rot = invParentRot * tr.rotation * invOrgRotations[bone];
            euler = MathfUtility.QuaternionToEuler(rot, MathfUtility.RotationOrder.YZX);
            invRot = Quaternion.identity;
            invRot = ApplyDirectRotation(invRot, euler.y, servos[ServoPosition.RightHipYaw]);
            invRot = ApplyDirectRotation(invRot, euler.z, servos[ServoPosition.RightHipRoll]);
            invRot = ApplyDirectRotation(invRot, euler.x, servos[ServoPosition.RightUpperLegPitch]);

            // 右脛姿勢を反映
            bone = HumanBodyBones.RightLowerLeg;
            tr = sourceHumanoid.GetBoneTransform(bone);
            rot = invRot * invParentRot * tr.rotation * invOrgRotations[bone];
            invRot = ApplyPartialRotation(rot, invRot, servos[ServoPosition.RightLowerLegPitch]);

            // 右足姿勢を反映
            bone = HumanBodyBones.RightFoot;
            tr = sourceHumanoid.GetBoneTransform(bone);
            rot = invRot * invParentRot * tr.rotation * invOrgRotations[bone];
            rot = ApplyPartialRotation(rot, servos[ServoPosition.RightFootPitch]);
            rot = ApplyPartialRotation(rot, servos[ServoPosition.RightFootRoll]);
        }

        /// <summary>
        /// 左脚以降の姿勢を反映
        /// </summary>
        void UpdateLeftLeg()
        {
            // 使いまわす変数
            HumanBodyBones bone;    // 対象ボーン
            Transform tr;           // 対象Transformの作業用変数
            Quaternion rot;         // 相対姿勢の作業用変数
            Vector3 euler;          // オイラー角で姿勢を表す際の変数
            Quaternion invRot;      // ワールド座標系の姿勢に直すための逆回転クォータニオン

            // 両脚の親
            Transform parentTransform = sourceHumanoid.GetBoneTransform(lowerBodyRootBone);
            Quaternion invParentRot = Quaternion.Inverse(parentTransform.rotation);

            // 左大腿姿勢を反映
            bone = HumanBodyBones.LeftUpperLeg;
            tr = sourceHumanoid.GetBoneTransform(bone);
            rot = invParentRot * tr.rotation * invOrgRotations[bone];
            euler = MathfUtility.QuaternionToEuler(rot, MathfUtility.RotationOrder.YZX);
            invRot = Quaternion.identity;
            invRot = ApplyDirectRotation(invRot, euler.y, servos[ServoPosition.LeftHipYaw]);
            invRot = ApplyDirectRotation(invRot, euler.z, servos[ServoPosition.LeftHipRoll]);
            invRot = ApplyDirectRotation(invRot, euler.x, servos[ServoPosition.LeftUpperLegPitch]);

            // 左脛姿勢を反映
            bone = HumanBodyBones.LeftLowerLeg;
            tr = sourceHumanoid.GetBoneTransform(bone);
            rot = invRot * invParentRot * tr.rotation * invOrgRotations[bone];
            invRot = ApplyPartialRotation(rot, invRot, servos[ServoPosition.LeftLowerLegPitch]);

            // 左足姿勢を反映
            bone = HumanBodyBones.LeftFoot;
            tr = sourceHumanoid.GetBoneTransform(bone);
            rot = invRot * invParentRot * tr.rotation * invOrgRotations[bone];
            rot = ApplyPartialRotation(rot, servos[ServoPosition.LeftFootPitch]);
            rot = ApplyPartialRotation(rot, servos[ServoPosition.LeftFootRoll]);
        }
    }
}

