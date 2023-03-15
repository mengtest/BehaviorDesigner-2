using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BehaviorDesigner
{
    public abstract class Behavior : MonoBehaviour, IBehavior
    {
        [SerializeField]
        private UpdateType updateType;
        [SerializeField]
        private bool restartWhenComplete;
        [SerializeField]
        private bool resetValuesOnRestart;
        [SerializeField]
        private int group;
        [SerializeField]
        private BehaviorSource source;
        [SerializeField]
        private ExternalBehavior external;
        [NonSerialized]
        private BehaviorSource externalSource;

        private TaskStatus status;
        private bool isInit;
        private bool isCompleted;

        public event Action<Behavior> OnBehaviorStart;
        public event Action<Behavior> OnBehaviorRestart;
        public event Action<Behavior> OnBehaviorEnd;

        public Root Root
        {
            get { return GetSource().Root; }
        }

        public string Name
        {
            get { return source.behaviorName; }
        }

        public int InstanceID
        {
            get { return GetInstanceID(); }
        }

        public bool RestartWhenComplete
        {
            get { return restartWhenComplete; }
            set { restartWhenComplete = value; }
        }

        public bool ResetValuesOnRestart
        {
            get { return resetValuesOnRestart; }
            set { resetValuesOnRestart = value; }
        }

        public int Group
        {
            get { return group; }
            set { group = value; }
        }

        public Object GetObject(bool local = false)
        {
            if (!local && external)
            {
                return external.GetObject();
            }
            
            return this;
        }

        public BehaviorSource GetSource(bool local = false)
        {
            if (!local && external)
            {
                if (Application.isPlaying)
                {
                    if (externalSource == null)
                    {
                        externalSource = external.GetSource().Clone();
                    }

                    return externalSource;
                }

                return external.GetSource();
            }

            if (source == null)
            {
                source = new BehaviorSource();
            }

            return source;
        }

        public void BindVariables(Task task)
        {
            GetSource().BindVariables(task);
        }

        public void SetExternalBehavior(ExternalBehavior behavior)
        {
            external = behavior;
            isInit = false;
        }

        public void Restart()
        {
            if (resetValuesOnRestart)
            {
                GetSource().ReloadVariables();
                Root?.Bind(GetSource());
            }

            isCompleted = false;
            Root?.OnStart();
            OnBehaviorRestart?.Invoke(this);
        }

        public void Tick()
        {
            if (!isInit)
            {
                Init();
            }

            if (isCompleted)
            {
                if (restartWhenComplete)
                {
                    Restart();
                }

                return;
            }

            status = Root.Update();
            if (status != TaskStatus.Running)
            {
                isCompleted = true;
                OnBehaviorEnd?.Invoke(this);
            }
        }

        public void LateTick()
        {
            if (!isInit || isCompleted)
            {
                return;
            }

            Root?.OnLateUpdate();
        }

        public void FixedTick()
        {
            if (!isInit || isCompleted)
            {
                return;
            }

            Root?.OnFixedUpdate();
        }

        private void Init()
        {
            isInit = true;
            isCompleted = false;
            GetSource().Load();
            Root.Bind(GetSource());
            Root.Init(this);
            OnBehaviorStart?.Invoke(this);
        }

        private void Update()
        {
            if (updateType == UpdateType.Auto)
            {
                Tick();
            }
        }

        private void LateUpdate()
        {
            if (updateType == UpdateType.Auto)
            {
                LateTick();
            }
        }

        private void FixedUpdate()
        {
            if (updateType == UpdateType.Auto)
            {
                FixedTick();
            }
        }

        private void OnDrawGizmos()
        {
            if (!isInit || isCompleted)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Root?.OnDrawGizmos();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isInit || isCompleted)
            {
                return;
            }

            Root?.OnCollisionEnter(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (!isInit || isCompleted)
            {
                return;
            }

            Root?.OnCollisionExit(collision);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isInit || isCompleted)
            {
                return;
            }

            Root?.OnTriggerEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!isInit || isCompleted)
            {
                return;
            }

            Root?.OnTriggerExit(other);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!isInit || isCompleted)
            {
                return;
            }

            Root?.OnCollisionEnter2D(collision);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (!isInit || isCompleted)
            {
                return;
            }

            Root?.OnCollisionExit2D(collision);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isInit || isCompleted)
            {
                return;
            }

            Root?.OnTriggerEnter2D(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!isInit || isCompleted)
            {
                return;
            }

            Root?.OnTriggerExit2D(other);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!isInit || isCompleted)
            {
                return;
            }

            Root?.OnControllerColliderHit(hit);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!isInit || isCompleted)
            {
                return;
            }

            Root?.OnAnimatorIK(layerIndex);
        }
    }
}