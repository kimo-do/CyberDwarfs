using Cysharp.Threading.Tasks;
using Solana.Unity.Extensions;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK.Example;
using Solana.Unity.SDK.Nft;
using Solana.Unity.SDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Linq;
using static NFTFetcher;
using Solana.Unity.Rpc.Models;

public class NFTFetcher : MonoBehaviour
{
    public static NFTFetcher instance;

    private CancellationTokenSource _stopTask;
    private List<TokenItem> _instantiatedTokens = new();
    private static TokenMintResolver _tokenResolver;
    private List<FetchedNFT> fetchedNFTs = new();

    public List<FetchedNFT> FetchedNFTs { get => fetchedNFTs; set => fetchedNFTs = value; }

    public class FetchedNFT
    {
        public Nft NFT;
        public TokenAccount account;
    }

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    public void RefreshWallet()
    {
        GetOwnedTokenAccounts().Forget();
    }

    private async UniTask GetOwnedTokenAccounts()
    {
        var tokens = await Web3.Wallet.GetTokenAccounts(Commitment.Confirmed);
        if (tokens == null) return;
        // Remove tokens not owned anymore and update amounts
        var tkToRemove = new List<TokenItem>();
        _instantiatedTokens.ForEach(tk =>
        {
            var tokenInfo = tk.TokenAccount.Account.Data.Parsed.Info;
            var match = tokens.Where(t => t.Account.Data.Parsed.Info.Mint == tokenInfo.Mint).ToArray();
            if (match.Length == 0 || match.Any(m => m.Account.Data.Parsed.Info.TokenAmount.AmountUlong == 0))
            {
                tkToRemove.Add(tk);
            }
            else
            {
                var newAmount = match[0].Account.Data.Parsed.Info.TokenAmount.UiAmountString;
                tk.UpdateAmount(newAmount);
            }
        });

        tkToRemove.ForEach(tk =>
        {
            _instantiatedTokens.Remove(tk);
            Destroy(tk.gameObject);
        });
        // Add new tokens
        if (tokens is { Length: > 0 })
        {
            var tokenAccounts = tokens.OrderByDescending(
                tk => tk.Account.Data.Parsed.Info.TokenAmount.AmountUlong);
            foreach (var item in tokenAccounts)
            {
                if (!(item.Account.Data.Parsed.Info.TokenAmount.AmountUlong > 0)) break;
                if (_instantiatedTokens.All(t => t.TokenAccount.Account.Data.Parsed.Info.Mint != item.Account.Data.Parsed.Info.Mint))
                {
                    //VisualNFT freeScreen = GetUnusedScreen();
                    //var tk = Instantiate(tokenItem, tokenContainer, true);
                    //tk.transform.localScale = Vector3.one;

                    Nft.TryGetNftData(item.Account.Data.Parsed.Info.Mint,
                        Web3.Instance.WalletBase.ActiveRpcClient).AsUniTask().ContinueWith(nft =>
                        {
                            fetchedNFTs.Add(new FetchedNFT()
                            { account = item, 
                              NFT = nft
                            });

                            //freeScreen.InitializeData(item, nft).Forget();


                            //TokenItem tkInstance = tk.GetComponent<TokenItem>();
                            //_instantiatedTokens.Add(tkInstance);
                            //tk.SetActive(true);
                            //if (tkInstance)
                            //{
                            //    tkInstance.InitializeData(item, this, nft).Forget();
                            //}
                        }).Forget();
                }
            }
        }
    }

    public static async UniTask<TokenMintResolver> GetTokenMintResolver()
    {
        if (_tokenResolver != null) return _tokenResolver;
        var tokenResolver = await TokenMintResolver.LoadAsync();
        if (tokenResolver != null) _tokenResolver = tokenResolver;
        return _tokenResolver;
    }
}
