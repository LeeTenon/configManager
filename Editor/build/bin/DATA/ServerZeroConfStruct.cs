using System;

/// <summary>
/// 日志输出模式
/// </summary>
public enum LogConfMode
{
    /// <summary>
    /// 命令行
    /// </summary>
    console = 0,

    /// <summary>
    /// 文件
    /// </summary>
    file,

    /// <summary>
    /// 自动识别 K8S pod 并按照不同 pod 分开写各自的日志文件
    /// </summary>
    volume,
}

/// <summary>
/// 日志输出等级
/// </summary>
public enum LogConfLevel
{
    /// <summary>
    /// info 开始所有日志输出
    /// </summary>
    info = 0,

    /// <summary>
    /// 仅输出 error
    /// </summary>
    error,

    /// <summary>
    /// 仅输出服务状态
    /// </summary>
    severe,
}


public enum ServiceConfMode
{
    /// <summary>
    /// 生产
    /// </summary>
    pro = 0,

    /// <summary>
    /// 开发
    /// </summary>
    dev,

    /// <summary>
    /// 测试
    /// </summary>
    test,

    /// <summary>
    /// 远程测试
    /// </summary>
    rt,

    /// <summary>
    /// 提前体验
    /// </summary>
    pre,
}

public enum EndpointMode
{
	/// <summary>
	/// 生产
	/// </summary>
	pro = 0,

	/// <summary>
	/// 开发
	/// </summary>
	dev,
}

public enum BoolValueWithDefault
{
    /// <summary>
    /// 使用默认值
    /// </summary>
    DEFAULT = 0,

    /// <summary>
    /// 否
    /// </summary>
    FALSE,

    /// <summary>
    /// 是
    /// </summary>
    TRUE,
}

public enum RedisConfType
{
    /// <summary>
    /// 节点
    /// </summary>
    node,

    /// <summary>
    /// 集群
    /// </summary>
    cluster,
}

public class ServerLogConf
{
    public string ServiceName;
    public ViEnum32<LogConfMode> Mode;
    public string TimeFormat;
    public string Path;
    public ViEnum32<LogConfLevel> Level;
    public ViEnum32<BoolValueWithDefault> Compress;
    public Int32 KeepDays;
    public Int32 StackCooldownMillis;
}

public class ServerPrometheusConf
{
    public string Host;
    public Int32 Port;
    public string Path;
}

public class ServerServiceConf : ViSealedData
{
    public ServerLogConf Log = new ServerLogConf();
    public ViEnum32<ServiceConfMode> Mode;
    public string MetricsUrl;
    public ServerPrometheusConf Prometheus = new ServerPrometheusConf();
}

public class ServerEtcdConf
{
    public string Hosts;
    public string Key;
}

public class ServerEtcdConfNokey
{
    public string Hosts;
}

public class ServerRedisConf
{
    public string Host;
    public ViEnum32<RedisConfType> Type;
    public string Pass;
    public ViEnum32<BoolValueWithDefault> Tls;
}

public class ServerRedisKeyConf : ServerRedisConf
{
    public string Key;
}

public class ServerRpcServerConf : ServerServiceConf
{
    public string ListenOn;
    public ServerEtcdConf Etcd = new ServerEtcdConf();
    public ViEnum32<BoolValueWithDefault> Auth;
    public ViEnum32<BoolValueWithDefault> StrictControl;
    public Int64 Timeout;
    public Int64 CpuThreshold;
}

public class ServerRestConf : ServerServiceConf
{
    public string Host;
    public Int32 Port;
    public string CertFile;
    public string KeyFile;
    public ViEnum32<BoolValueWithDefault> Verbose;
    public Int32 MaxConns;
    public Int64 MaxBytes;
    public Int64 Timeout;
    public Int64 CpuThreshold;
    // 遗漏了 Signature    SignatureConf
}

public class ServerRpcClientConf
{
    public ServerEtcdConf Etcd = new ServerEtcdConf();
    public string Endpoints;
    public string App;
    public string Token;
    public Int64 Timeout;
	public string ConnectInterval;
}

public class RabbitConf
{
	public string Host;
	public Int32 HttpPort;
	public string User;
	public string Password;
	public Int32 MqQos;
}

public class ClientRpcConfWithMq : ServerRpcClientConf
{
	public Int32 MqChIdleLimit;
	public Int64 MqRpcHeartbeat;
}

public class ServerServiceConfigWithMq: ServerRpcServerConf
{
	public RabbitConf Rabbit = new RabbitConf();
}
