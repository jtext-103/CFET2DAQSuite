using Jtext103.CFET2.Things.BasicAIModel;
using NationalInstruments.ModularInstruments.NIScope;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.CFET2.Things.NIScopeDAQAI
{
    public class NIScopeAI : IBasicAI
    {
        //每个采集卡一个实例
        private NIScope scopeSession;

        private Status aiState;

        private NIScopeAIStaticConfig staticConfig;

        #region public AI status
        public Status AIState
        {
            get
            {
                return aiState;
            }
            private set
            {
                if (aiState != value)
                {
                    aiState = value;

                    //状态改变时，产生OnStatusChanged事件
                    //todo: OnStatusChanged();
                }
            }
        }

        /// <summary>
        /// 获取上一炮时间
        /// </summary>
        public DateTime LastShotTime { get; private set; }

        /// <summary>
        /// 获取采集相关配置
        /// </summary>
        public BasicAIStaticConfig StaticConfig
        {
            get
            {
                return staticConfig;
            }
        }

        /// <summary>
        /// 采集数据是否需要转置：不需要
        /// </summary>
        public bool DataNeedTransposeWhenSaving
        {
            get
            {
                return false;
            }
        }
        #endregion

        #region 事件相关
        public event EventHandler RaiseAITaskStopEvent;
        public event EventHandler RaiseStatusChangeEvent;
        public event EventHandler<DataArrivalEventArgs> RaiseDataArrivalEvent;
        #endregion

        public NIScopeAI()
        {
            LastShotTime = DateTime.UtcNow;
            aiState = Status.Idle;
        }

        /// <summary>
        /// 从配置文件为已经存在的 basicAI 赋初值
        /// </summary>
        /// <param name="configFilePath"></param>
        public void InitAI(string configFilePath)
        {
            staticConfig = LoadStaticConfig(configFilePath) as NIScopeAIStaticConfig;
        }

        /// <summary>
        /// 实例化新的 AI，可以从配置文件生成有内容的，也可以是空的
        /// </summary>
        /// <param name="configFilePath">给 null 或 "" 表示实例化一个空的，否则给配置文件路径</param>
        /// <returns></returns>
        public BasicAIStaticConfig LoadStaticConfig(string configFilePath)
        {
            if (configFilePath == "" || configFilePath == null)
            {
                return new NIScopeAIStaticConfig();
            }
            return new NIScopeAIStaticConfig(configFilePath);
        }

        /// <summary>
        /// 实例化并启动采集任务
        /// </summary>
        public void TryArmTask()
        {
            if (AIState != Status.Idle)
            {
                throw new Exception("If you want to arm, the AI state must be 'Idle'!");
            }
            else
            {
                if (scopeSession == null)
                {
                    try
                    {
                        scopeSession = new NIScope(staticConfig.ResourceName, false, false);
                    }
                    catch(Exception e)
                    {
                        throw new Exception(e.ToString());
                    }
                }
            }
        }

        public bool TryStopTask()
        {
            throw new NotImplementedException();
        }
    }
}
