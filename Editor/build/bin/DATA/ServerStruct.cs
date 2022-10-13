using System;

#region BaseConfig definition
public class ServiceBaseConfig: ServerRpcServerConf
{
	public RabbitConf Rabbit = new RabbitConf();
	public DatabaseConfig Database = new DatabaseConfig();
	public CommonConfig Common = new CommonConfig();

}

public class CommonConfig
{
	public string TimeZone;
	public string ServerCallTimeout;
}

public class DatabaseConfig
{
	public MysqlClientConfig Mysql = new MysqlClientConfig();
	public RedisClientConfig Redis = new RedisClientConfig();
	public StorageConfig Storage = new StorageConfig();
}

public class MysqlClientConfig
{
	public string Host;
	public int MaxOpenConnections;
	public int MaxIdleConnections;
	public string MaxConnectionLifetime;
}

public class RedisClientConfig
{
	public string Host;
	public string Pass;
	public ViEnum32<BoolValueWithDefault> Verbose;
	public ViEnum32<BoolValueWithDefault> Tls;
	public ViEnum32<BoolValueWithDefault> TlsSkipVerify;
	public string TlsHandshakeTimeout;
	public Int32 MaxIdle;
	public Int32 MaxActive;
	public string IdleTimeout;
	public string ConnectTimeout;
	public string ReadTimeout;
	public string WriteTimeout;
}

public enum EStorageType
{
	memory = 0,
	dynamo
}

public class StorageConfig
{
	public ViEnum32<EStorageType> StorageType;
	public string Host;
	public string Endpoint;
	public string Prefix;
	public int MaxLength;
	public string Tick;
}

public class TaskHelperConfig
{
	public Int32 Threshold;
	public Int32 Threads;
	public string Deadline;
	public string RetryInterval;

	public Int32 UpdateRetryTimes;
	public Int32 InsertRetryTimes;
}

public class MQConfig
{
	public int Threshold;
	public int Threads;
	public int RetryTimes;
}
#endregion

#region Bots definition
public partial class ServerBotsYamlStruct : ServerRestConf
{
    public string BotType;
    public string LoginEndpoint;
    public string GateEndpoint;
}
#endregion

#region Gate definition
public partial class ServerGateYamlStruct : ServiceBaseConfig
{
	public GateSpec Spec = new GateSpec();
}
public class GateSpec
{
	public TcpServerConfig Server = new TcpServerConfig();
	public GateRpcConfig Rpc = new GateRpcConfig();
}

public class GateRpcConfig
{
	public Int32 MemoryBufferLength;
	public Int32 Threshold;
	public Int32 Threads;
	public Int32 RetryThreshold;
	public Int32 MqChannelLimit;
	public Int64 MqRpcHeartbeat;
	public Int64 Timeout;
}

public class TcpServerServiceConfig
{
	public string RpcTimeout;
	public string RpcSlowDuration;
	public Int32 ServiceMaxNumber;
	public string UpdateInterval;
	public string ConnectInterval;
	public string JudgeInterval;
	public Int32 MsgBufferSize;
}

public class TcpServerGetEndpointConfig
{
	public string UpdateInterval;
	public string Hosts;
}

public class TcpServerConfig
{
	public string Host;
	public Int32 Port;
	public string TokenDecryptKey;
	public string TokenTimeout;
	public TcpServerServiceConfig Service = new TcpServerServiceConfig();
	public TcpServerGetEndpointConfig GetEndpoint = new TcpServerGetEndpointConfig();
}
#endregion

#region Gm definition
public class TelnetConfig
{
	public Int32 Port;
}

public partial class ServerGmYamlStruct : ServerRestConf
{
	public string DataSource;
	public ServerEtcdConfNokey Etcd = new ServerEtcdConfNokey();
	public string UpdateInterval;
	public TelnetConfig Telnet = new TelnetConfig();
	public GMAuthConfig Auth = new GMAuthConfig();
	public GMAccountConfig GMAdmin = new GMAccountConfig();
	public LogsCleanerConfig LogsCleaner = new LogsCleanerConfig();
	public RedisClientConfig RedisConfig = new RedisClientConfig();
	public string ResetSecret;
}

public class LogsCleanerConfig
{
	public string LogDuration;
	public string CleanerDuration;
}
public class GMAccountConfig
{
	public string GMName;
	public string GMPassword;
	public string Email;
}

public class GMAuthConfig
{
	public string AccessSecret;
	public Int64 AccessExpire;
}

public class OauthConfig
{
	public string OauthType;
	public string ClienId;
	public string ClientSecret;
	public string RedirectUrl;

}
#endregion

#region Login definition
public partial class ServerLoginYamlStruct : ServerRestConf
{
	public RedisClientConfig RedisConfig = new RedisClientConfig();
	public LoginGateConfig Gate = new LoginGateConfig();
	public LoginAuthConfig Auth = new LoginAuthConfig();
	public LoginOfficialConfig Official = new LoginOfficialConfig();
	public LoginWeGameConfig WeGame = new LoginWeGameConfig();
	public LoginSteamConfig Steam = new LoginSteamConfig();
	public LoginTokenConfig Token = new LoginTokenConfig();
	public ClientRpcConfWithMq AccountRpc = new ClientRpcConfWithMq();
	public RabbitConf Rabbit = new RabbitConf();
	public ViEnum32<EndpointMode> EndpointMode;
}

public class LoginGateConfig
{
	public string UpdateInterval;
	public string StartTime;
}

public class LoginOfficialConfig
{
}

public class LoginTokenConfig
{
	public string GateTokenExpireTime;
	public string LoginTokenExpireTime;
	public string TokenEncryptKey;
}

public class LoginWeGameConfig
{
	public string GameId;
	public string ClientId;
	public string ClientSecret;
}

public class LoginSteamConfig
{
	public string AppId;
	public string UserKey;
	public string PublisherKey;
	public string SkipLoginAuth;
}

public class LoginAuthConfig
{
	public string AccessSecret;
	public Int64 AccessExpire;
}

#endregion

#region Account definition
public partial class ServerAccountYamlStruct : ServiceBaseConfig
{
	public AccountSpec Spec = new AccountSpec();
}

public class AccountSpec
{
	public ViEnum32<IdGeneretorType> IdGeneratorType;
	public AccountInLineConfig InLine = new AccountInLineConfig();
}

public enum IdGeneretorType
{
	uuid = 0,
}

public enum InLineConfigType
{
	lazy = 0,
	infinite,
}

public class AccountInLineConfig
{
	public ViEnum32<InLineConfigType> Type;
	public Int32 MaxPlayer;
	public Int32 AlertPercent;
	public string UpdateInterval;
	public string ExpireTime;
}
#endregion

#region Analytics definition
public partial class ServerAnalyticsYamlStruct : ServiceBaseConfig
{
	public AnalyticsSpec Spec = new AnalyticsSpec();
}

public class AnalyticsSpec
{
	public AnalyticsDailyReportConfig DailyReport = new AnalyticsDailyReportConfig();
	public AnalyticsRpcConfig Rpc = new AnalyticsRpcConfig();
}

public class AnalyticsDailyReportConfig
{
	public string UpdateInterval;
	public string PreUpdateTime;
}

public class AnalyticsRpcConfig
{
	public string GateHealthTime;
	public string RpcInterval;
	public Int32 BufferMaxLength;
	public string RpcTimeout;
}
#endregion

#region Assets definition
public partial class ServerAssetsYamlStruct : ServiceBaseConfig
{
}
#endregion

#region Chat definition
public partial class ServerChatYamlStruct : ServiceBaseConfig
{
	public ChatSpec Spec = new ChatSpec();
}

public partial class ChatSpec
{
	public string SendMessageTimeout;
	public string ChatRoomExpireTime;
	public string UserChatInfoExpireTime;
	public ViEnum32<SensitiveWordsFilterType> SensitiveWordsFilterType;
	public int MaxTrySaveNum;
}
public enum SensitiveWordsFilterType
{
	/**
	 * 不过滤敏感词
	 */
	NoFiltering = 0,
}

#endregion

#region Collector definition
public partial class ServerCollectorYamlStruct : ServiceBaseConfig
{
	public CollectorSpec Spec = new CollectorSpec();
}
public partial class CollectorSpec
{
	public string SendDataTimeout;
	public MatchDataHandlerConfig MatchDataHandler = new MatchDataHandlerConfig();
}
public class MatchDataHandlerConfig
{
	public string MaxDowntime;
	public string MaxChannelLimit;
}
#endregion

#region Friend definition
public partial class ServerFriendshipYamlStruct : ServiceBaseConfig
{
	public FriendshipSpec Spec = new FriendshipSpec();
}
public class FriendshipSpec
{
	public string NewRpcClientInterval;
	public FriendBufferConfig FriendBuffer = new FriendBufferConfig();
	public FriendServerCallConfig ServerCall = new FriendServerCallConfig();
}

public class FriendBufferConfig
{
	public Int32 Size;
}

public class FriendServerCallConfig
{
	public string SendMessageTimeout;
	public Int32 RetryTimes;
}
#endregion

#region Matchmaking definition
public enum MatchmakingModeSet
{
	/// <summary>
	/// unity matchmaking
	/// </summary>
	unity,
}

public enum MatchmakingUnityRegion
{
	/// <summary>
	/// 中国
	/// </summary>
	China = 0,

	/// <summary>
	/// 海外
	/// </summary>
	Oversea,
}

public class MatchmakingUnityEndpoint
{
	public string China;
	public string Oversea;
}

public class MatchmakingUnityAuthentication
{
	public MatchmakingUnityEndpoint Endpoint = new MatchmakingUnityEndpoint();
}

public class MatchmakingUnityMatchmaking
{
	public MatchmakingUnityEndpoint Endpoint = new MatchmakingUnityEndpoint();
}

public class MatchmakingUnityClientItem
{
	public string ID;
	public string Secret;
}

public class MatchmakingUnityClient
{
	public MatchmakingUnityClientItem China = new MatchmakingUnityClientItem();
	public MatchmakingUnityClientItem Oversea = new MatchmakingUnityClientItem();
}

public class MatchmakingUnityConfig
{
	public MatchmakingUnityAuthentication Authentication = new MatchmakingUnityAuthentication();
	public MatchmakingUnityMatchmaking Matchmaking = new MatchmakingUnityMatchmaking();
	public MatchmakingUnityClient Client = new MatchmakingUnityClient();
	public ViEnum32<MatchmakingUnityRegion> Region;
	public string RequestTimeout;
	public string QueryTicketTimeout;
	public string DeleteTicketAfterMatchmakingSucceedDelayTime;
}

public partial class ServerMatchmakingYamlStruct : ServiceBaseConfig
{
	public MatchmakingSpec Spec = new MatchmakingSpec();
}

public class MatchmakingSpec
{
	public ViEnum32<MatchmakingModeSet> MatchmakingMode;
	public string SendMessageTimeout;
	public MQConfig MessageQueue = new MQConfig();
	public MatchmakingUnityConfig Unity = new MatchmakingUnityConfig();
}
#endregion

#region Challenge definition
public partial class ServerChallengeYamlStruct : ServiceBaseConfig
{
}
#endregion

#region PlayerDate definition
public partial class ServerPlayerdataYamlStruct : ServiceBaseConfig
{
	public PlayerdataDataConfig Data = new PlayerdataDataConfig();
}
public class PlayerdataDataConfig
{
	public Int32 MemoryBufferLength;
	public Int32 Threshold;
	public Int32 Threads;
	public string RpcTimeout;
	public Int32 RetryThreshold;
}
#endregion

#region Profile definition
public partial class ServerProfileYamlStruct : ServiceBaseConfig
{
	public ProfileSpec Spec = new ProfileSpec();
}

public partial class ProfileSpec
{
	public string SendMessageTimeout;
	MQConfig MqConfig = new MQConfig();
}
#endregion

#region Session definition
public partial class ServerSessionYamlStruct : ServiceBaseConfig
{
	public int RedisRetryTimes;
	public int RedisLockExpire;
}
#endregion

#region Trade definition
public partial class ServerTradeYamlStruct : ServiceBaseConfig
{
}
#endregion

#region Party definition
public class ServerPartyYamlStruct : ServiceBaseConfig
{
	public PartySpec Spec = new PartySpec();
}
public class PartySpec
{
	public string SendMessageTimeout;
	public string PartyExpireTime;
	public string UserPartyStatusExpireTime;
	public string UserPendingInviteExpireTime;
}
#endregion

#region Feedback definition
public partial class ServerFeedbackYamlStruct : ServiceBaseConfig
{
	public FeedbackSpec Spec = new FeedbackSpec();
}

public class FeedbackSpec
{
	public int QueryLimit;
	public string IsPrintDBDebugInfo;
}
#endregion

#region Recent definition
public class ServerRecentYamlStruct : ServiceBaseConfig
{
}
#endregion

// 这个配置仅用作链接服务器使用�? 配置组，不生成具体配置文�?
#region Other/Test
public partial class ServerConfigSelectionStruct : ViSealedData
{
	public Int32 DefaultConfigId;
	public ViForeignKey<ServerBotsYamlStruct> BotsConfigId;
	public ViForeignKey<ServerLoginYamlStruct> LoginConfigId;
	public ViForeignKey<ServerGateYamlStruct> GateConfigId;
	public ViForeignKey<ServerGmYamlStruct> GmConfigId;
	public ViForeignKey<ServerAccountYamlStruct> AccountConfigId;
	public ViForeignKey<ServerAnalyticsYamlStruct> AnalyticsConfigId;
	public ViForeignKey<ServerAssetsYamlStruct> AssetsConfigId;
	public ViForeignKey<ServerChatYamlStruct> ChatConfigId;
	public ViForeignKey<ServerCollectorYamlStruct> CollectorConfigId;
	public ViForeignKey<ServerFriendshipYamlStruct> FriendshipConfigId;
	public ViForeignKey<ServerMatchmakingYamlStruct> MatchmakingConfigId;
	public ViForeignKey<ServerPlayerdataYamlStruct> PlayerdataConfigId;
	public ViForeignKey<ServerProfileYamlStruct> ProfileConfigId;
	public ViForeignKey<ServerSessionYamlStruct> SessionConfigId;
	public ViForeignKey<ServerTradeYamlStruct> TradeConfigId;
	public ViForeignKey<ServerChallengeYamlStruct> ChallengeConfigId;
	public ViForeignKey<ServerPartyYamlStruct> PartyConfigId;
	public ViForeignKey<ServerFeedbackYamlStruct> FeedbackConfigId;
	public ViForeignKey<ServerRecentYamlStruct> RecentConfigId;
}

// ---------- 测试所有字段，正式不会删除但请勿删�? ---------- //
// ---------- 已知问题，数组显示未找到好办法，选择使用 string 类型并使用`;`分隔 ---------- //

public class TestLevel2Struct
{
	public string TestLevel2String;
	public Int32 TestLevel2Int32;
}

public class TestLevel1Struct
{
	public string TestLevel1String;
	public Int32 TestLevel1Int32;
	public TestLevel2Struct TestLevel2Deep = new TestLevel2Struct();
	public string TestArray;
}

public partial class ServerTestYamlStruct : ViSealedData
{
	public string TestString;
	public Int64 TestAdd;
	public Int32 TestInt32;
	public TestLevel1Struct TestLevel1Deep = new TestLevel1Struct();
	public ServerLogConf Log = new ServerLogConf();
}
#endregion

