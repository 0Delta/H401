using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class GameEffect : MonoBehaviour {

    private static string[] PARTICLE_CONTROLLER_NAMES = {
        "KiraKiraController",
        "Flower0Controller",
    };
    
    private enum _eParticleType {
        KIRAKIRA = 0,
        FLOWER_0,

        MAX
    }

    private enum _eParticleEffectType {
        MOVE_TIMEGAUGE,
        MOVE_FEVERGAUGE,

        NONE,
        MAX
    }

    private struct ParticlePool {
        public GameObject     obj;
        public bool           isUse;
    }

    [SerializeField] private float effectPopPosZ = 0.0f;
    [SerializeField] private float effectMoveDurationTime = 0.0f;
    [SerializeField] private float effectMoveWaitTime = 0.0f;
    [SerializeField] private int[] effectOverScore;
    [SerializeField] private string[] particlePaths;
    [SerializeField] private int[] particlePoolSize;
    
    private GameObject[] particleController;
    private GameObject[] particlePrefabs;
    private ParticleSystem[] particleSystems;
    private ParticlePool[][] particlePool;
    private int[] poolSearchID;
    private Vector3 limitTimePos;
    private Vector3 feverGaugePos;

    void Awake() {
        // Pool のメモリを確保
        particlePool = new ParticlePool[(int)_eParticleType.MAX][];
        for(int i = 0; i < (int)_eParticleType.MAX; ++i) {
            particlePool[i] = new ParticlePool[particlePoolSize[i]];
        }
    }

    void Start() {
        // ゲームシーンを取得
        GameObject gameScene = transform.root.GetComponent<AppliController>().GetCurrentScene();
        GameScene gameSceneScript = gameScene.GetComponent<GameScene>();

        // パーティクルコントローラーを生成
        particleController = new GameObject[(int)_eParticleType.MAX];
        for(int i = 0; i < (int)_eParticleType.MAX; ++i) {
            particleController[i] = new GameObject();
            particleController[i].transform.SetParent(transform.parent);
            particleController[i].name = PARTICLE_CONTROLLER_NAMES[i];
        }

        // パーティクルの prefab を取得
        particlePrefabs = new GameObject[particlePaths.Length];
        particleSystems = new ParticleSystem[particlePaths.Length];
        for(int i = 0; i < particlePaths.Length; ++i) {
            particlePrefabs[i] = Resources.Load<GameObject>(particlePaths[i]);
            particleSystems[i] = particlePrefabs[i].transform.GetComponent<ParticleSystem>();
        }

        // ParticlePool を初期化
        for(int i = 0; i < (int)_eParticleType.MAX; ++i) {
            for(int j = 0; j < particlePoolSize[i]; ++j) {
                particlePool[i][j].obj = (GameObject)Instantiate(particlePrefabs[i], particleController[i].transform.position, particleController[i].transform.rotation);
                particlePool[i][j].obj.transform.SetParent(particleController[i].transform);
                particlePool[i][j].obj.SetActive(false);
                particlePool[i][j].isUse = false;
            }
        }

        // poolSearchID を初期化
        poolSearchID = new int[(int)_eParticleType.MAX];
        for(int i = 0; i < (int)_eParticleType.MAX; ++i)
            poolSearchID[i] = 0;

        // 各種ゲージの position を取得
        GameInfoCanvas gic = gameSceneScript.gameUI.gameInfoCanvas;
        limitTimePos = gic.limitTime.transform.position;
        limitTimePos.z = effectPopPosZ;
        feverGaugePos = gic.feverGauge.transform.position;
        feverGaugePos.z = effectPopPosZ;
    }

    // ツリー完成時の演出処理
    public void TreeCompleteEffect(List<GameObject> nodes, int score) {
        foreach(var node in nodes) {
            // エフェクト出現
            Vector3 pos = node.transform.position;
            pos.z = effectPopPosZ;

            // キラキラエフェクト
            SearchParticlePool(pos, (int)_eParticleType.KIRAKIRA, _eParticleEffectType.MOVE_TIMEGAUGE);  // 体力ゲージ
            SearchParticlePool(pos, (int)_eParticleType.KIRAKIRA, _eParticleEffectType.MOVE_FEVERGAUGE);  // フィーバーゲージ
            
            Node nodeScript = node.GetComponent<Node>();
            // 枝先か壁ならエフェクトを出現
            if(nodeScript.Temp.LinkNum == 1 || nodeScript.Temp.LinkNum >= 3 || nodeScript.CheckLinkedWall()) {
                int i = -1;
                while(i < (int)_eParticleType.MAX - 1) {
                    if(score < effectOverScore[i + 1]) {
                        break;
                    }

                    ++i;
                }

                // 花エフェクト
                if(i > 0) {
//                    SearchParticlePool(pos, i);
                }
            }
        }
    }

    void SearchParticlePool(Vector3 appearPos, int type, _eParticleEffectType pet = _eParticleEffectType.NONE) {
        for(int i = 0; i < particlePoolSize[type]; ++i) {
            if(particlePool[type][poolSearchID[type]].isUse != true) {
                particlePool[type][poolSearchID[type]].obj.transform.position = appearPos;
                particlePool[type][poolSearchID[type]].obj.SetActive(true);
                particlePool[type][poolSearchID[type]].isUse = true;

                switch(pet) {
                    case _eParticleEffectType.MOVE_TIMEGAUGE:
                        MoveFinishedBanish(type, poolSearchID[type], limitTimePos, effectMoveDurationTime, effectMoveWaitTime);
                        break;

                    case _eParticleEffectType.MOVE_FEVERGAUGE:
                        MoveFinishedBanish(type, poolSearchID[type], feverGaugePos, effectMoveDurationTime, effectMoveWaitTime);
                        break;

                    default:
                        break;
                }

                ++poolSearchID[type];
                if(poolSearchID[type] >= particlePoolSize[type])
                    poolSearchID[type] = 0;

                break;
            }
        }
    }
    
    void MoveFinishedBanish(int type, int poolID, Vector3 endPos, float duration, float waitTime) {
        particlePool[type][poolID].obj.transform.DOMove(endPos, duration)
            .OnComplete(() => {
                particlePool[type][poolID].obj.transform.DOScale(0.0f, waitTime)
                .OnComplete(() => {
                    particlePool[type][poolID].obj.SetActive(false);
                    particlePool[type][poolID].isUse = false;
                });
            })
            .SetEase(Ease.InCubic);
    }
}
