using NBitcoin;
using QBitNinja.Client;
using System;
using System.Linq;
using static System.Console;

namespace TumbleBitTinkering
{
    class Program
    {
		private static readonly Network _network = Network.TestNet;
		private static readonly BitcoinExtKey _seed = new BitcoinExtKey("tprv8ZgxMBicQKsPeGSjHbcTdpBxmVvRmySiUkQFBLruRTrn2dXAtn2rqApjVUgqsFkhfJZLYy8kXgRaEgZh7M3zdxyabF1TcwoxZsgpmAnHYyH");
		private static readonly ExtKey _fundingExtKey = _seed.ExtKey.Derive(0, false);
		private static readonly BitcoinAddress _fundingAddress = _fundingExtKey.ScriptPubKey.GetDestinationAddress(_network); // mkvRuHAv3qek4mP3ipjqFnaFXj4d2kKit3
		private static readonly QBitNinjaClient _qBitClient = new QBitNinjaClient(_network);
		
		static void Main(string[] args)
        {
			SpendBasicTransaction();
			ReadKey();
        }

		private static void SpendBasicTransaction()
		{
			uint256 txIdToSpend = _qBitClient.GetBalance(_fundingAddress, unspentOnly: false).Result.Operations.First().TransactionId;
			Transaction txToSpend = _qBitClient.GetTransaction(txIdToSpend).Result.Transaction;

			Money fee = new Money(0.001m, MoneyUnit.BTC);
			var builder = new TransactionBuilder();
			var txThatSpends = builder
				.AddCoins(txToSpend)
				.AddKeys(_fundingExtKey)
				.SendFees(fee)
				.Send(_fundingAddress, txToSpend.Outputs.First().Value - fee)
				.BuildTransaction(true);

			var errors = builder.Check(txThatSpends);
			if (errors.Count() != 0)
			{
				WriteLine("Check failed. Errors:");
				foreach (var err in errors)
				{
					WriteLine(err);
				}
			}
			else
			{
				WriteLine(txThatSpends);
			}
		}
	}
}