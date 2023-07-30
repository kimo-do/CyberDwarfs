using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Solana.Unity.Extensions.Models.TokenMint;
using Solana.Unity.Rpc.Models;
using Solana.Unity.SDK.Nft;
using Solana.Unity.SDK.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class VisualNFT : MonoBehaviour
{
    //public TextMeshProUGUI pub_txt;
    //public TextMeshProUGUI ammount_txt;

    //public RawImage logo;
    public SpriteRenderer sr;
    public GameObject glitchOverlay;
    public Vector3 overrideScale = new Vector3(1.2f, 1.2f, 1);

    public GameObject fallback;

    //public Button transferButton;

    public TokenAccount TokenAccount;
    private Nft _nft;
    private Texture2D _texture;

    private void Awake()
    {
        //logo = GetComponentInChildren<RawImage>();
    }

    public void SetNFT(TokenAccount tokenAccount, Solana.Unity.SDK.Nft.Nft nftData = null)
    {
        gameObject.SetActive(true);
        sr.gameObject.SetActive(false);
        glitchOverlay.SetActive(false);
        InitializeData(tokenAccount, nftData).Forget();
    }

    public async UniTask InitializeData(TokenAccount tokenAccount, Solana.Unity.SDK.Nft.Nft nftData = null)
    {
        TokenAccount = tokenAccount;
        if (nftData != null && ulong.Parse(tokenAccount.Account.Data.Parsed.Info.TokenAmount.Amount) == 1)
        {
            await UniTask.SwitchToMainThread();
            _nft = nftData;
            //ammount_txt.text = "";
            //pub_txt.text = nftData.metaplexData?.data?.offchainData?.name;

            Texture2D tex = nftData.metaplexData?.nftImage?.file;

            if (tex != null)
            {
                sr.sprite = CreateSpriteFromTexture(tex);
                sr.transform.localScale = overrideScale;
                fallback.SetActive(false);
                sr.gameObject.SetActive(true);
                glitchOverlay.SetActive(true);
            }
            //if (logo != null)
            //{
            //    logo.texture = nftData.metaplexData?.nftImage?.file;
            //}

        }
        else
        {
            //ammount_txt.text =
            //    tokenAccount.Account.Data.Parsed.Info.TokenAmount.AmountDecimal.ToString(CultureInfo
            //        .CurrentCulture);
            //pub_txt.text = nftData?.metaplexData?.data?.offchainData?.name ?? tokenAccount.Account.Data.Parsed.Info.Mint;
            //if (nftData?.metaplexData?.data?.offchainData?.symbol != null)
            //{
            //    pub_txt.text += $" ({nftData?.metaplexData?.data?.offchainData?.symbol})";
            //}

            if (nftData?.metaplexData?.data?.offchainData?.default_image != null)
            {
                await LoadAndCacheTokenLogo(nftData.metaplexData?.data?.offchainData?.default_image, tokenAccount.Account.Data.Parsed.Info.Mint);
            }
            else
            {
                var tokenMintResolver = await NFTFetcher.GetTokenMintResolver();
                TokenDef tokenDef = tokenMintResolver.Resolve(tokenAccount.Account.Data.Parsed.Info.Mint);
                if (tokenDef.TokenName.IsNullOrEmpty() || tokenDef.Symbol.IsNullOrEmpty()) return;
                //pub_txt.text = $"{tokenDef.TokenName} ({tokenDef.Symbol})";
                await LoadAndCacheTokenLogo(tokenDef.TokenLogoUrl, tokenDef.TokenMint);
            }
        }
    }

    private async Task LoadAndCacheTokenLogo(string logoUrl, string tokenMint)
    {
        if (logoUrl.IsNullOrEmpty() || tokenMint.IsNullOrEmpty()) return;
        var texture = await FileLoader.LoadFile<Texture2D>(logoUrl);
        _texture = FileLoader.Resize(texture, 75, 75);
        FileLoader.SaveToPersistentDataPath(Path.Combine(Application.persistentDataPath, $"{tokenMint}.png"), _texture);
        Sprite sprite = CreateSpriteFromTexture(_texture);
        sr.sprite = sprite;
        //logo.texture = _texture;
    }

    public Sprite CreateSpriteFromTexture(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    //public void UpdateAmount(string newAmount)
    //{
    //    MainThreadDispatcher.Instance().Enqueue(() => { ammount_txt.text = newAmount; });
    //}
}
