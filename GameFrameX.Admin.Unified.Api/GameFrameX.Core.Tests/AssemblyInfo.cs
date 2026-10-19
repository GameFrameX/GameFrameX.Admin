

// CryptogramUtility 等工具持有静态状态（CryptoType/PublicKey/PrivateKey），
// 测试需串行执行以避免跨类静态状态竞态
[assembly: CollectionBehavior(DisableTestParallelization = true)]