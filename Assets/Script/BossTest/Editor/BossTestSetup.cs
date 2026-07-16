#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BossTestSetup
{
    const string Root = "Assets/Data/FSM/Boss";

    [MenuItem("Tools/FSM/Boss Test/1. Create Boss Graph + States")]
    public static void CreateBossGraph()
    {
        EnsureFolder("Assets/Data");
        EnsureFolder("Assets/Data/FSM");
        EnsureFolder(Root);
        EnsureFolder(Root + "/States");

        // --- Actions ---
        var roar = CreateOrLoadSo<RoarStateSo>(Root + "/States/Boss_Roar.asset", so => so.duration = 1.0f);
        var stalk = CreateOrLoadSo<StalkStateSo>(Root + "/States/Boss_Stalk.asset", so =>
        {
            so.duration = 0.95f;
            so.orbitSpeed = 6.2f;
            so.approachSpeed = 4.2f;
            so.preferDistance = 5.8f;
            so.forceDecideDistance = 4.2f;
        });
        var combo = CreateOrLoadSo<ComboMeleeStateSo>(Root + "/States/Boss_Combo.asset", so =>
        {
            so.hitCount = 3;
            so.damage = 7f;
            so.hitRange = 3.6f;
        });
        var heavy = CreateOrLoadSo<HeavySlashStateSo>(Root + "/States/Boss_Heavy.asset", so =>
        {
            so.windup = 0.7f;
            so.damage = 22f;
            so.hitRange = 4.2f;
        });
        var charge = CreateOrLoadSo<ChargeStateSo>(Root + "/States/Boss_Charge.asset", so =>
        {
            so.style = ChargeStyle.Random;
            so.chargeSpeed = 15f;
            so.duration = 0.85f;
            so.damage = 16f;
            so.hitRadius = 2.2f;
            so.zigzagAmp = 5f;
            so.zigzagFreq = 15f;
            so.curveTurnRate = 240f;
        });
        var chargeP3 = CreateOrLoadSo<ChargeStateSo>(Root + "/States/Boss_Charge_P3.asset", so =>
        {
            so.style = ChargeStyle.Double;
            so.chargeSpeed = 21f;
            so.duration = 1.1f;
            so.damage = 22f;
            so.hitRadius = 2.5f;
        });
        var chargeZig = CreateOrLoadSo<ChargeStateSo>(Root + "/States/Boss_Charge_Zig.asset", so =>
        {
            so.style = ChargeStyle.Zigzag;
            so.chargeSpeed = 16f;
            so.duration = 0.9f;
            so.damage = 17f;
            so.hitRadius = 2.15f;
            so.zigzagAmp = 6f;
            so.zigzagFreq = 16f;
        });
        var chargeCurve = CreateOrLoadSo<ChargeStateSo>(Root + "/States/Boss_Charge_Curve.asset", so =>
        {
            so.style = ChargeStyle.Curve;
            so.chargeSpeed = 15.5f;
            so.duration = 0.9f;
            so.damage = 17f;
            so.hitRadius = 2.2f;
            so.curveTurnRate = 280f;
        });
        var chargeFeint = CreateOrLoadSo<ChargeStateSo>(Root + "/States/Boss_Charge_Feint.asset", so =>
        {
            so.style = ChargeStyle.Feint;
            so.chargeSpeed = 17f;
            so.duration = 0.95f;
            so.damage = 18f;
            so.hitRadius = 2.25f;
        });
        var barrage = CreateOrLoadSo<BarrageStateSo>(Root + "/States/Boss_Barrage.asset", so =>
        {
            so.pattern = BarragePattern.Random;
            so.shotCount = 8;
            so.interval = 0.08f;
            so.spreadAngle = 38f;
            so.damage = 5f;
            so.projectileSpeed = 15f;
            so.ringCount = 12;
            so.waveCount = 3;
            so.strafeSpeed = 3.2f;
        });
        var barrageP3 = CreateOrLoadSo<BarrageStateSo>(Root + "/States/Boss_Barrage_P3.asset", so =>
        {
            so.pattern = BarragePattern.Storm;
            so.shotCount = 14;
            so.interval = 0.08f;
            so.spreadAngle = 50f;
            so.damage = 6f;
            so.projectileSpeed = 18f;
            so.ringCount = 16;
            so.waveCount = 5;
            so.windup = 0.28f;
            so.recover = 0.28f;
            so.strafeSpeed = 6.2f;
        });
        var barrageP3Spiral = CreateOrLoadSo<BarrageStateSo>(Root + "/States/Boss_Barrage_P3_Spiral.asset", so =>
        {
            so.pattern = BarragePattern.Spiral;
            so.shotCount = 16;
            so.interval = 0.055f;
            so.spreadAngle = 40f;
            so.damage = 5f;
            so.projectileSpeed = 16.5f;
            so.ringCount = 14;
            so.waveCount = 4;
            so.windup = 0.22f;
            so.recover = 0.22f;
            so.strafeSpeed = 7f;
        });
        var barrageP3Cross = CreateOrLoadSo<BarrageStateSo>(Root + "/States/Boss_Barrage_P3_Cross.asset", so =>
        {
            so.pattern = BarragePattern.Cross;
            so.shotCount = 12;
            so.interval = 0.09f;
            so.spreadAngle = 42f;
            so.damage = 6f;
            so.projectileSpeed = 17.5f;
            so.ringCount = 12;
            so.waveCount = 4;
            so.windup = 0.25f;
            so.recover = 0.25f;
            so.strafeSpeed = 5.5f;
        });
        var barrageP3Bloom = CreateOrLoadSo<BarrageStateSo>(Root + "/States/Boss_Barrage_P3_Bloom.asset", so =>
        {
            so.pattern = BarragePattern.Bloom;
            so.shotCount = 12;
            so.interval = 0.085f;
            so.spreadAngle = 48f;
            so.damage = 6f;
            so.projectileSpeed = 18f;
            so.ringCount = 16;
            so.waveCount = 5;
            so.windup = 0.3f;
            so.recover = 0.28f;
            so.strafeSpeed = 5.8f;
        });
        var slam = CreateOrLoadSo<SlamStateSo>(Root + "/States/Boss_Slam.asset", so =>
        {
            so.windup = 0.8f;
            so.damage = 20f;
            so.radius = 4.5f;
        });
        var backstep = CreateOrLoadSo<BackstepStateSo>(Root + "/States/Boss_Backstep.asset", so =>
        {
            so.duration = 0.35f;
            so.speed = 11f;
        });
        var recover = CreateOrLoadSo<RecoverStateSo>(Root + "/States/Boss_Recover.asset", so => so.duration = 0.5f);
        var recoverShort = CreateOrLoadSo<RecoverStateSo>(Root + "/States/Boss_Recover_Short.asset", so => so.duration = 0.16f);
        var enrage = CreateOrLoadSo<EnrageStateSo>(Root + "/States/Boss_Enrage.asset", so =>
        {
            so.duration = 1.15f;
            so.speedMul = 1.5f;
            so.damageMul = 1.4f;
        });
        var closeIn = CreateOrLoadSo<ApproachStateSo>(Root + "/States/Boss_CloseIn.asset", so =>
        {
            so.engageDistance = 3.8f;
            so.moveSpeed = 7.5f;
        });

        // --- Specials ---
        var blink = CreateOrLoadSo<BlinkStrikeStateSo>(Root + "/States/Boss_Blink.asset", so => { });
        var spin = CreateOrLoadSo<SpinRushStateSo>(Root + "/States/Boss_Spin.asset", so =>
        {
            so.duration = 1.35f;
            so.moveSpeed = 7.2f;
            so.spinSpeed = 820f;
            so.tickInterval = 0.16f;
            so.damage = 4f;
            so.hitRadius = 2.9f;
        });
        var spinP3 = CreateOrLoadSo<SpinRushStateSo>(Root + "/States/Boss_Spin_P3.asset", so =>
        {
            so.duration = 1.55f;
            so.moveSpeed = 9f;
            so.spinSpeed = 980f;
            so.tickInterval = 0.14f;
            so.damage = 5f;
            so.hitRadius = 3.1f;
        });
        var vacuum = CreateOrLoadSo<VacuumStateSo>(Root + "/States/Boss_Vacuum.asset", so => { });
        var nova = CreateOrLoadSo<NovaRingStateSo>(Root + "/States/Boss_Nova.asset", so => { });
        var meteor = CreateOrLoadSo<MeteorRainStateSo>(Root + "/States/Boss_Meteor.asset", so =>
        {
            so.count = 12;
            so.windup = 0.35f;
            so.interval = 0.1f;
            so.recover = 0.28f;
            so.damage = 7f;
            so.fallSpeed = 14f;
        });
        var homing = CreateOrLoadSo<HomingVolleyStateSo>(Root + "/States/Boss_Homing.asset", so =>
        {
            so.count = 6;
            so.interval = 0.1f;
            so.speed = 6f;
            so.turnRate = 185f;
            so.damage = 8f;
        });
        var homingP3 = CreateOrLoadSo<HomingVolleyStateSo>(Root + "/States/Boss_Homing_P3.asset", so =>
        {
            so.count = 10;
            so.windup = 0.28f;
            so.interval = 0.07f;
            so.recover = 0.22f;
            so.speed = 7.5f;
            so.turnRate = 220f;
            so.damage = 8f;
        });
        var laser = CreateOrLoadSo<LaserSweepStateSo>(Root + "/States/Boss_Laser.asset", so =>
        {
            so.previewDuration = 0.55f;
            so.holdDuration = 0.32f;
            so.strikeDuration = 0.2f;
            so.recover = 0.28f;
            so.damage = 22f;
            so.beamLength = 13f;
            so.beamWidth = 0.9f;
            so.sweepAngle = 125f;
            so.strikeCount = 1;
        });
        var laserP3 = CreateOrLoadSo<LaserSweepStateSo>(Root + "/States/Boss_Laser_P3.asset", so =>
        {
            so.previewDuration = 0.5f;
            so.holdDuration = 0.28f;
            so.strikeDuration = 0.16f;
            so.recover = 0.22f;
            so.damage = 26f;
            so.beamLength = 14f;
            so.beamWidth = 1.05f;
            so.sweepAngle = 150f;
            so.strikeCount = 2; // 왕복 긁기
        });

        // --- Compares ---
        var cmpPhase3 = CreateOrLoadSo<CompareStateSo>(Root + "/States/Boss_Cmp_Phase3.asset", so =>
        {
            so.compareOperator = CompareOperatorType.GreaterOrEqual;
            so.leftKey = BlackboardKey.BossPhase;
            so.rightKey = 3f;
        });
        var cmpPhase2 = CreateOrLoadSo<CompareStateSo>(Root + "/States/Boss_Cmp_Phase2.asset", so =>
        {
            so.compareOperator = CompareOperatorType.GreaterOrEqual;
            so.leftKey = BlackboardKey.BossPhase;
            so.rightKey = 2f;
        });
        var cmpMelee = CreateOrLoadSo<CompareStateSo>(Root + "/States/Boss_Cmp_Melee.asset", so =>
        {
            so.compareOperator = CompareOperatorType.LessOrEqual;
            so.leftKey = BlackboardKey.DistToPlayer;
            so.rightKey = 3.5f;
        });
        var cmpMid = CreateOrLoadSo<CompareStateSo>(Root + "/States/Boss_Cmp_Mid.asset", so =>
        {
            so.compareOperator = CompareOperatorType.LessOrEqual;
            so.leftKey = BlackboardKey.DistToPlayer;
            so.rightKey = 9f;
        });
        var cmpMeleeP2 = CreateOrLoadSo<CompareStateSo>(Root + "/States/Boss_Cmp_Melee_P2.asset", so =>
        {
            so.compareOperator = CompareOperatorType.LessOrEqual;
            so.leftKey = BlackboardKey.DistToPlayer;
            so.rightKey = 4.2f;
        });
        var cmpMidP2 = CreateOrLoadSo<CompareStateSo>(Root + "/States/Boss_Cmp_Mid_P2.asset", so =>
        {
            so.compareOperator = CompareOperatorType.LessOrEqual;
            so.leftKey = BlackboardKey.DistToPlayer;
            so.rightKey = 11f;
        });
        var cmpCloseP3 = CreateOrLoadSo<CompareStateSo>(Root + "/States/Boss_Cmp_Close_P3.asset", so =>
        {
            so.compareOperator = CompareOperatorType.LessOrEqual;
            so.leftKey = BlackboardKey.DistToPlayer;
            so.rightKey = 5f;
        });

        var monEnrage = CreateOrLoadSo<MonitorStateSo>(Root + "/States/Boss_Mon_Enrage.asset", so =>
        {
            so.compareOperator = CompareOperatorType.LessOrEqual;
            so.leftKey = BlackboardKey.HpRatio;
            so.rightKey = 65f; // HP 65% 이하에서 1회 Enrage 컷신
        });

        string graphPath = Root + "/Boss FSM Graph.asset";
        var graph = AssetDatabase.LoadAssetAtPath<FSMGraphSo>(graphPath);
        if (graph == null)
        {
            graph = ScriptableObject.CreateInstance<FSMGraphSo>();
            AssetDatabase.CreateAsset(graph, graphPath);
        }

        graph.nodes = new List<NodeData>();
        graph.edges = new List<EdgeData>();

        float x0 = 0, x1 = 220, x2 = 460, x3 = 720, x4 = 980, x5 = 1240, x6 = 1500, x7 = 1760;

        var entry = AddNode(graph, "entryNode", NodeType.Entry, "Entry", new Vector2(x0, 240), null);
        graph.entryNodeId = entry.id;

        var nRoar = AddNode(graph, "boss_roar", NodeType.Action, "Roar", new Vector2(x1, 240), roar);
        var nStalk = AddNode(graph, "boss_stalk", NodeType.Action, "Stalk", new Vector2(x2, 240), stalk);

        var nCmpP3 = AddNode(graph, "boss_cmp_p3", NodeType.Transition, "Phase>=3?", new Vector2(x3, 240), cmpPhase3);
        var nCmpP2 = AddNode(graph, "boss_cmp_p2", NodeType.Transition, "Phase>=2?", new Vector2(x3, 420), cmpPhase2);

        // Phase1 hubs
        var nP1Melee = AddNode(graph, "boss_p1_melee", NodeType.Transition, "P1 Dist<=Melee", new Vector2(x4, 40), cmpMelee);
        var nP1Mid = AddNode(graph, "boss_p1_mid", NodeType.Transition, "P1 Dist<=Mid", new Vector2(x4, 160), cmpMid);
        var nCombo = AddNode(graph, "boss_combo", NodeType.Action, "ComboMelee", new Vector2(x5, 0), combo);
        var nCharge = AddNode(graph, "boss_charge", NodeType.Action, "Charge(Rnd)", new Vector2(x5, 100), charge);
        var nChargeZig = AddNode(graph, "boss_charge_zig", NodeType.Action, "ZigZagCharge", new Vector2(x5, 180), chargeZig);
        var nChargeZig2 = AddNode(graph, "boss_charge_zig2", NodeType.Action, "ZigZag(P2)", new Vector2(x7, 400), chargeZig);
        var nBarrage = AddNode(graph, "boss_barrage", NodeType.Action, "Barrage(Rnd)", new Vector2(x5, 260), barrage);
        var nBackstep = AddNode(graph, "boss_backstep", NodeType.Action, "Backstep", new Vector2(x6, 260), backstep);
        var nRecover = AddNode(graph, "boss_recover", NodeType.Action, "Recover", new Vector2(x6, 80), recover);

        // Phase2 — special mix (Vacuum은 루프에서 제외 → Enrage 때만)
        var nP2Melee = AddNode(graph, "boss_p2_melee", NodeType.Transition, "P2 Dist<=Melee", new Vector2(x4, 360), cmpMeleeP2);
        var nP2Mid = AddNode(graph, "boss_p2_mid", NodeType.Transition, "P2 Dist<=Mid", new Vector2(x4, 500), cmpMidP2);
        var nSlam = AddNode(graph, "boss_slam", NodeType.Action, "Slam", new Vector2(x5, 320), slam);
        var nChargeFeint = AddNode(graph, "boss_charge_feint", NodeType.Action, "FeintCharge", new Vector2(x5, 380), chargeFeint);
        var nHeavy = AddNode(graph, "boss_heavy", NodeType.Action, "HeavySlash", new Vector2(x6, 300), heavy);
        var nSpin = AddNode(graph, "boss_spin", NodeType.Action, "SpinRush", new Vector2(x7, 300), spin);
        var nLaser = AddNode(graph, "boss_laser", NodeType.Action, "LaserSweep", new Vector2(x5, 460), laser);
        var nHoming = AddNode(graph, "boss_homing", NodeType.Action, "HomingOrbs", new Vector2(x6, 460), homing);
        var nBlink = AddNode(graph, "boss_blink", NodeType.Action, "BlinkStrike", new Vector2(x5, 580), blink);
        var nNova = AddNode(graph, "boss_nova", NodeType.Action, "NovaRing", new Vector2(x6, 580), nova);
        var nChargeCurve = AddNode(graph, "boss_charge_curve", NodeType.Action, "CurveCharge", new Vector2(x7, 580), chargeCurve);
        var nRecover2 = AddNode(graph, "boss_recover2", NodeType.Action, "Recover(P2)", new Vector2(x7, 460), recoverShort);

        // Phase3 — 탄막 다발 + 기동 콤보 (close / mid / far)
        var nP3Close = AddNode(graph, "boss_p3_close", NodeType.Transition, "P3 Dist<=Close", new Vector2(x4, 720), cmpCloseP3);
        var nP3Mid = AddNode(graph, "boss_p3_mid", NodeType.Transition, "P3 Dist<=Mid", new Vector2(x4, 860), cmpMidP2);
        var nMeteor = AddNode(graph, "boss_meteor", NodeType.Action, "MeteorRain", new Vector2(x5, 660), meteor);
        var nBarrage3 = AddNode(graph, "boss_barrage3", NodeType.Action, "StormBarrage", new Vector2(x6, 640), barrageP3);
        var nBlink3 = AddNode(graph, "boss_blink3", NodeType.Action, "Blink(P3)", new Vector2(x7, 620), blink);
        var nSpin3 = AddNode(graph, "boss_spin3", NodeType.Action, "Spin(P3)", new Vector2(x7 + 220, 620), spinP3);
        var nCharge3 = AddNode(graph, "boss_charge3", NodeType.Action, "DoubleCharge", new Vector2(x7 + 440, 620), chargeP3);
        var nHoming3 = AddNode(graph, "boss_homing3", NodeType.Action, "Homing(P3)", new Vector2(x5, 820), homingP3);
        var nBarrage3Spiral = AddNode(graph, "boss_barrage3_spiral", NodeType.Action, "SpiralBarrage", new Vector2(x6, 820), barrageP3Spiral);
        var nChargeZig3 = AddNode(graph, "boss_charge_zig3", NodeType.Action, "ZigZag(P3)", new Vector2(x7, 820), chargeZig);
        var nBarrage3Cross = AddNode(graph, "boss_barrage3_cross", NodeType.Action, "CrossBarrage", new Vector2(x7 + 220, 820), barrageP3Cross);
        var nChargeCurve3 = AddNode(graph, "boss_charge_curve3", NodeType.Action, "Curve(P3)", new Vector2(x7 + 440, 820), chargeCurve);
        var nLaser3 = AddNode(graph, "boss_laser3", NodeType.Action, "Laser(P3)", new Vector2(x5, 980), laserP3);
        var nBarrage3Bloom = AddNode(graph, "boss_barrage3_bloom", NodeType.Action, "BloomBarrage", new Vector2(x6, 980), barrageP3Bloom);
        var nVacuum3 = AddNode(graph, "boss_vacuum3", NodeType.Action, "Vacuum(P3)", new Vector2(x7, 980), vacuum);
        var nCharge3Far = AddNode(graph, "boss_charge3_far", NodeType.Action, "FeintCharge(P3)", new Vector2(x7 + 220, 980), chargeFeint);
        var nNova3 = AddNode(graph, "boss_nova3", NodeType.Action, "Nova(P3)", new Vector2(x7 + 440, 980), nova);
        var nRecover3 = AddNode(graph, "boss_recover3", NodeType.Action, "Recover(P3)", new Vector2(x7 + 660, 820), recoverShort);

        var nVacuum = AddNode(graph, "boss_vacuum", NodeType.Action, "Vacuum", new Vector2(x3, 40), vacuum);
        var nEnrage = AddNode(graph, "boss_enrage", NodeType.Action, "Enrage", new Vector2(x2, 40), enrage);
        var nMonEnrage = AddNode(graph, "boss_mon_enrage", NodeType.Monitor, "Mon HP<=65%", new Vector2(x1, 40), monEnrage);
        var nMonDesperate = AddNode(graph, "boss_mon_desp", NodeType.Monitor, "Mon HP<=30%", new Vector2(x1, -40),
            CreateOrLoadSo<MonitorStateSo>(Root + "/States/Boss_Mon_Desperate.asset", so =>
            {
                so.compareOperator = CompareOperatorType.LessOrEqual;
                so.leftKey = BlackboardKey.HpRatio;
                so.rightKey = 30f;
            }));
        var nMeteorCut = AddNode(graph, "boss_meteor_cut", NodeType.Action, "DesperateMeteor", new Vector2(x2, -40), meteor);

        var nRefStalk = AddNode(graph, "boss_ref_stalk", NodeType.Reference, "Ref → Stalk", new Vector2(x7, 240), null);
        nRefStalk.referenceTargetId = nStalk.id;

        // Entry flow
        AddEdge(graph, entry.id, nRoar.id, PortType.Output, "Out");
        AddEdge(graph, nRoar.id, nStalk.id, PortType.Output, "Out");
        AddEdge(graph, nStalk.id, nCmpP3.id, PortType.Output, "Out");
        AddEdge(graph, nCmpP3.id, nP3Close.id, PortType.True, "True");
        AddEdge(graph, nCmpP3.id, nCmpP2.id, PortType.False, "False");
        AddEdge(graph, nCmpP2.id, nP2Melee.id, PortType.True, "True");
        AddEdge(graph, nCmpP2.id, nP1Melee.id, PortType.False, "False");

        // P1
        AddEdge(graph, nP1Melee.id, nCombo.id, PortType.True, "True");
        AddEdge(graph, nP1Melee.id, nP1Mid.id, PortType.False, "False");
        AddEdge(graph, nCombo.id, nCharge.id, PortType.Output, "Out");
        AddEdge(graph, nCharge.id, nRecover.id, PortType.Output, "Out");
        AddEdge(graph, nP1Mid.id, nChargeZig.id, PortType.True, "True");
        AddEdge(graph, nP1Mid.id, nBarrage.id, PortType.False, "False");
        AddEdge(graph, nChargeZig.id, nRecover.id, PortType.Output, "Out");
        AddEdge(graph, nBarrage.id, nBackstep.id, PortType.Output, "Out");
        AddEdge(graph, nBackstep.id, nRefStalk.id, PortType.Output, "Out");
        AddEdge(graph, nRecover.id, nRefStalk.id, PortType.Output, "Out");

        // P2: Slam→FeintCharge→Heavy→Spin | Laser→Homing→ZigZag | Blink→Nova→CurveCharge
        AddEdge(graph, nP2Melee.id, nSlam.id, PortType.True, "True");
        AddEdge(graph, nP2Melee.id, nP2Mid.id, PortType.False, "False");
        AddEdge(graph, nP2Mid.id, nLaser.id, PortType.True, "True");
        AddEdge(graph, nP2Mid.id, nBlink.id, PortType.False, "False");
        AddEdge(graph, nSlam.id, nChargeFeint.id, PortType.Output, "Out");
        AddEdge(graph, nChargeFeint.id, nHeavy.id, PortType.Output, "Out");
        AddEdge(graph, nHeavy.id, nSpin.id, PortType.Output, "Out");
        AddEdge(graph, nSpin.id, nRecover2.id, PortType.Output, "Out");
        AddEdge(graph, nLaser.id, nHoming.id, PortType.Output, "Out");
        AddEdge(graph, nHoming.id, nChargeZig2.id, PortType.Output, "Out");
        AddEdge(graph, nChargeZig2.id, nRecover2.id, PortType.Output, "Out");
        AddEdge(graph, nBlink.id, nNova.id, PortType.Output, "Out");
        AddEdge(graph, nNova.id, nChargeCurve.id, PortType.Output, "Out");
        AddEdge(graph, nChargeCurve.id, nRecover2.id, PortType.Output, "Out");
        AddEdge(graph, nRecover2.id, nRefStalk.id, PortType.Output, "Out");

        // P3 close: Meteor→Storm→Blink→Spin→Double
        // P3 mid: Homing→Spiral→ZigZag→Cross→Curve
        // P3 far: Laser→Bloom→Vacuum→Feint→Nova
        AddEdge(graph, nP3Close.id, nMeteor.id, PortType.True, "True");
        AddEdge(graph, nP3Close.id, nP3Mid.id, PortType.False, "False");
        AddEdge(graph, nP3Mid.id, nHoming3.id, PortType.True, "True");
        AddEdge(graph, nP3Mid.id, nLaser3.id, PortType.False, "False");
        AddEdge(graph, nMeteor.id, nBarrage3.id, PortType.Output, "Out");
        AddEdge(graph, nBarrage3.id, nBlink3.id, PortType.Output, "Out");
        AddEdge(graph, nBlink3.id, nSpin3.id, PortType.Output, "Out");
        AddEdge(graph, nSpin3.id, nCharge3.id, PortType.Output, "Out");
        AddEdge(graph, nCharge3.id, nRecover3.id, PortType.Output, "Out");
        AddEdge(graph, nHoming3.id, nBarrage3Spiral.id, PortType.Output, "Out");
        AddEdge(graph, nBarrage3Spiral.id, nChargeZig3.id, PortType.Output, "Out");
        AddEdge(graph, nChargeZig3.id, nBarrage3Cross.id, PortType.Output, "Out");
        AddEdge(graph, nBarrage3Cross.id, nChargeCurve3.id, PortType.Output, "Out");
        AddEdge(graph, nChargeCurve3.id, nRecover3.id, PortType.Output, "Out");
        AddEdge(graph, nLaser3.id, nBarrage3Bloom.id, PortType.Output, "Out");
        AddEdge(graph, nBarrage3Bloom.id, nVacuum3.id, PortType.Output, "Out");
        AddEdge(graph, nVacuum3.id, nCharge3Far.id, PortType.Output, "Out");
        AddEdge(graph, nCharge3Far.id, nNova3.id, PortType.Output, "Out");
        AddEdge(graph, nNova3.id, nRecover3.id, PortType.Output, "Out");
        AddEdge(graph, nRecover3.id, nRefStalk.id, PortType.Output, "Out");

        // Monitors — Vacuum은 Enrage 직후 1회만
        AddEdge(graph, nMonEnrage.id, nEnrage.id, PortType.Output, "Out");
        AddEdge(graph, nEnrage.id, nVacuum.id, PortType.Output, "Out");
        AddEdge(graph, nVacuum.id, nRefStalk.id, PortType.Output, "Out");
        AddEdge(graph, nMonDesperate.id, nMeteorCut.id, PortType.Output, "Out");
        AddEdge(graph, nMeteorCut.id, nRefStalk.id, PortType.Output, "Out");

        EditorUtility.SetDirty(graph);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = graph;
        Debug.Log(
            "보스 FSM 갱신: P3 탄막 3루트 + 전페이즈 기동 강화\n" +
            "메뉴 1번 재실행 후 Play.");
    }

    [MenuItem("Tools/FSM/Boss Test/2. Setup Scene 2D (Player + Boss)")]
    public static void SetupScene()
    {
        EnsureFolder(Root);

        var graph = AssetDatabase.LoadAssetAtPath<FSMGraphSo>(Root + "/Boss FSM Graph.asset");
        if (graph == null)
        {
            EditorUtility.DisplayDialog("Boss Test", "먼저 1번 Create Boss Graph 를 실행하세요.", "OK");
            return;
        }

        // 기존 3D 테스트 오브젝트 정리(있을 때만)
        var oldGround = GameObject.Find("Ground");
        if (oldGround != null)
            Object.DestroyImmediate(oldGround);

        // 예전 테스트 적 — HUD가 Find 잘못 잡는 원인
        var oldTestEnemy = GameObject.Find("TestEnemy");
        if (oldTestEnemy != null)
            oldTestEnemy.SetActive(false);

        var playerGo = EnsureActor2D("Player", new Vector3(0f, 0f, 0f), new Color(0.45f, 1.35f, 1.9f), 1f);
        if (playerGo.GetComponent<PlayerController>() == null)
            playerGo.AddComponent<PlayerController>();

        var bossGo = EnsureActor2D("Boss", new Vector3(0f, 8f, 0f), new Color(2.1f, 0.25f, 0.45f), 1.6f);
        var enemy = bossGo.GetComponent<EnemyController>();
        if (enemy == null)
            enemy = bossGo.AddComponent<EnemyController>();

        enemy.graphSo = graph;
        enemy.enemyStat = new BaseStat
        {
            name = "Boss",
            maxHp = 200f,
            hp = 200f,
            detectionDistance = 20f,
            moveSpeed = 5.8f
        };

        if (Camera.main != null)
        {
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = 9f;
            Camera.main.transform.position = new Vector3(0f, 0f, -10f);
            Camera.main.transform.rotation = Quaternion.identity;
            if (Camera.main.GetComponent<CombatCamera>() == null)
                Camera.main.gameObject.AddComponent<CombatCamera>();
        }

        if (Object.FindObjectOfType<SpaceBackdrop>() == null)
            new GameObject("SpaceBackdrop").AddComponent<SpaceBackdrop>();

        var hudGo = GameObject.Find("BossCombatHud");
        if (hudGo == null)
        {
            hudGo = new GameObject("BossCombatHud");
            hudGo.AddComponent<BossCombatHud>();
        }
        else if (hudGo.GetComponent<BossCombatHud>() == null)
        {
            hudGo.AddComponent<BossCombatHud>();
        }

        var hud = hudGo.GetComponent<BossCombatHud>();
        if (hud != null)
            hud.SetBoss(enemy);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = bossGo;
        Debug.Log("2D 씬 셋업 완료. WASD 이동 / 마우스 조준 / 좌클릭 사격.");
    }

    static GameObject EnsureActor2D(string name, Vector3 pos, Color color, float scale)
    {
        var go = GameObject.Find(name);
        if (go == null)
        {
            go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * scale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = BossTestSprites.Circle;
            sr.color = color;
            sr.sortingOrder = 5;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        else
        {
            go.transform.position = pos;
            // 3D Capsule 등에서 바꿨을 수 있어 2D 구성 보정
            Object.DestroyImmediate(go.GetComponent<MeshFilter>());
            Object.DestroyImmediate(go.GetComponent<MeshRenderer>());
            Object.DestroyImmediate(go.GetComponent<Collider>());
            Object.DestroyImmediate(go.GetComponent<Rigidbody>());

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = BossTestSprites.Circle;
            sr.color = color;

            if (go.GetComponent<CircleCollider2D>() == null)
                go.AddComponent<CircleCollider2D>().radius = 0.5f;

            var rb = go.GetComponent<Rigidbody2D>();
            if (rb == null) rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        return go;
    }

    static NodeData AddNode(FSMGraphSo graph, string id, NodeType type, string title, Vector2 pos, BaseStateSoAsset so)
    {
        var node = new NodeData
        {
            id = id,
            nodeType = type,
            title = title,
            position = pos,
            stateSo = so
        };
        graph.nodes.Add(node);
        return node;
    }

    static void AddEdge(FSMGraphSo graph, string from, string to, PortType outPort, string outName)
    {
        graph.edges.Add(new EdgeData
        {
            outputNodeId = from,
            inputNodeId = to,
            outputPortType = outPort,
            inputPortType = PortType.Input,
            outPortName = outName
        });
    }

    static T CreateOrLoadSo<T>(string path, System.Action<T> setup) where T : ScriptableObject
    {
        var so = AssetDatabase.LoadAssetAtPath<T>(path);
        if (so == null)
        {
            so = ScriptableObject.CreateInstance<T>();
            setup?.Invoke(so);
            AssetDatabase.CreateAsset(so, path);
        }
        else
        {
            setup?.Invoke(so);
            EditorUtility.SetDirty(so);
        }
        return so;
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        string name = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}
#endif
