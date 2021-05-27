# Scuti Unity SDK
Scuti is a monetization platform designed to enhance player experiences with your games. 

![Category_Sports](https://user-images.githubusercontent.com/16213480/119724555-154ae380-be2c-11eb-8717-57a006b519da.png)

Gamers can opt-in to Scuti by visiting the Scuti store. The store is filled with physical products, video trailers, and online offers. They can earn blockchain backed 'Scutis' by making purchases, logging in daily, and watching video ads. These Scutis can then be exchanged for in-game currency or used to making other purchases.

Developers get a cut of all purchases made and videos watched from within their game. They also get 100% of the revenue from any Scutis that are exchanged for their game's native currency.

**The Integration Process**:

1. Create a game developer account on Scuti's web dashboard
2. Add the ScutiSDK package to your project using UPM 
3. Add your AppId to the ScutiSettings.asset
4. Place the Scuti Store prefab button in your shop
5. Add currency exchange listeners either on the client side or server side

When the gamer clicks/taps the Scuti Store prefab button, it will trigger the Scuti store to open and overlay the screen. While the store is not being actively viewed by the gamer it will remain dormant (outside of initializion/login calls on startup). It will not take any processing or cache any images and thus will not impact your game in any way. 

Please check out our integration guide below for a complete walk through of each step. 

[Integration Guide](https://github.com/scuti-ai/scuti-sdk/wiki/Integration-Guide)

**Dependencies**
- Newtonsoft's JSON library. 
- BouncyCastle. We are using a subset of their library so we can encrypt credit card data. If you are using their library you can delete our BouncyCastle.dll as it will not have the entire library (we are trying to keep our filesize down). 
