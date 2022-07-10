# Roid1 Simpole

Unity でインポートした URDF に、Unity の Humanoid の姿勢を真似させます。

現状、 @Ninagawa123 さんの roid1_udf 専用に調整した状態です。


# 動作確認
再生するとキャラクターのモーションを真似るようにロボットが動きます。

ロボットにアタッチされている Controller の Speed や Force Limit などで挙動が変わります。

キャラクターの AnimatorController は無効にし、再生中に上腕、前腕や足等をインスペクタで回転させると、関節毎の動作を確認できます。



# URDF を初めから準備する場合

## URDF インポーターをパッケージマネージャで追加
1. パッケージマネージャで Add Package from Git URL を選択
2. 次のURLを入れてAdd  https://github.com/Unity-Technologies/URDF-Importer.git?path=/com.unity.robotics.urdf-importer


## URDF の用意
@Ninagawa123 さんのURDFを元にしていますが、下記では一部リンク名を修正しています。
https://github.com/kirurobo/Roid1_URDF

そのままではUnityでのインポートに失敗したため、
- roid1.urdf
- roid1_urdf/urdf/*.stl
と、.urdf ファイルからの相対パスが roid1_urdf/urdf/ 下となるようにSTLを配置する形で、Assets フォルダ内に保存します。


## インポート
Unityのプロジェクト欄で roid1.urdf を右クリックし、
「Import Robot from Selected URDF file」を選択。

設定はおそらくデフォルトでよい。
- Select Axis Type は「Y Axis」
- Mesh Decomposer は「VHACD」


## ロボットモデル配置後の設定
ヒエラルキーにroid1が現れたら、ヒエラルキーとインスペクタから下記を行なっておきます。
- roid1のUrdfRobot で「Use Gravity」は Disable をしておく
  - コライダーがないため画面外に落ちてしまいます
- roid1のController で Stiffnes や Speed 等を適当な値に設定する
  - 初期値だと Speed がゼロなどで、目標角度が指定されても動きません
- ヒエラルキーから c_waist を選び、Articulation Body の Immovable にチェックをつける
  - つけないと重力なしでは徐々に浮かんでいきます


# スクリプトの用意
Scripts/HumanoidMimiCryController.cs をロボットにアタッチします。

Souce Humanoid に、動作元となるモデルを指定します。