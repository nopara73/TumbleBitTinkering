using NBitcoin;
using NBitcoin.Policy;
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
			BuildProofOfTumbleBitConceptTransactions();
			ReadKey();
		}

		private static void BuildProofOfTumbleBitConceptTransactions()
		{
			uint256 txIdToSpend = _qBitClient.GetBalance(_fundingAddress, unspentOnly: false).Result.Operations.First().TransactionId;
			Transaction txToSpend = _qBitClient.GetTransaction(txIdToSpend).Result.Transaction;

			var tOfferExtKey = _seed.ExtKey.Derive(1, false);
			var tOfferDestination = tOfferExtKey.ScriptPubKey.GetDestinationAddress(_network); // mk1soVb7Se1t99v7APGqiXofr2pKVS7hN5
			var tFulfillExtKey = _seed.ExtKey.Derive(2, false);
			var tFulfillDestination = tFulfillExtKey.ScriptPubKey.GetDestinationAddress(_network); // n4mey3skuXQna149vUaanuAPyEDiunsryT

			var fee = new Money(0.001m, MoneyUnit.BTC);
			var builder = new TransactionBuilder();
			var tOffer = builder
				.AddCoins(txToSpend)
				.AddKeys(_fundingExtKey)
				.SendFees(fee)
				.Send(tOfferDestination, txToSpend.Outputs.First().Value - fee)
				.BuildTransaction(sign: true);

			var errors = builder.Check(tOffer);
			if (errors.Count() != 0)
			{
				ReportErrors(txName: nameof(tOffer), errors: errors);
			}
			else
			{
				ReportTransaction(txName: nameof(tOffer), transaction: tOffer);
			}

			builder = new TransactionBuilder();
			var tFulfill = builder
				.AddCoins(tOffer)
				.AddKeys(tOfferExtKey)
				.SendFees(fee)
				.Send(tFulfillDestination, tOffer.Outputs.First().Value - fee)
				.BuildTransaction(sign: true);

			errors = builder.Check(tFulfill);
			if (errors.Count() != 0)
			{
				ReportErrors(txName: nameof(tFulfill), errors: errors);
			}
			else
			{
				ReportTransaction(txName: nameof(tFulfill), transaction: tOffer);
			}
		}

		private static void ReportTransaction(string txName, Transaction transaction)
		{
			WriteLine();
			WriteLine();
			WriteLine($"{txName}:");
			WriteLine();
			WriteLine(transaction);
			WriteLine();
		}

		private static void ReportErrors(string txName, TransactionPolicyError[] errors)
		{
			WriteLine($"{txName} check failed. Errors:");
			foreach (var err in errors)
			{
				WriteLine(err);
			}
		}
	}
}