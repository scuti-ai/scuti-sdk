#import "ScutiNativeAppstore.h"

@implementation ScutiNativeAppstore

- (id)init
{
    self = [super init];
    
    return self;
}

-(void) openAppInStore: (int) appID
{
    if ([SKStoreProductViewController class])
    {
        NSDictionary *parameters = @{SKStoreProductParameterITunesItemIdentifier: [NSNumber numberWithInteger: appID]};
        
        SKStoreProductViewController *productViewController = [[SKStoreProductViewController alloc] init];
        [productViewController loadProductWithParameters:parameters completionBlock:nil];
        [productViewController setDelegate:self];
        [UnityGetGLViewController() presentViewController:productViewController animated:YES completion:nil];
    }
    
    else
    {
        
        [[UIApplication sharedApplication] openURL:[NSURL URLWithString: [NSString stringWithFormat: @"http://itunes.apple.com/app/id%d?mt=8", appID]]];
    }
}

-(void)productViewControllerDidFinish:(SKStoreProductViewController *)viewController
{   [viewController dismissViewControllerAnimated:YES completion:nil];
    
    
    UnitySendMessage("~AppstoreHandler", "appstoreClosed", "");
}

- (BOOL)prefersStatusBarHidden
{   return YES;
}

@end

static ScutiNativeAppstore *scutiNativeAppstorePlugin = nil;

extern "C"
{
    void _ScutiOpenAppInStore(int appID)
    {
        NSLog(@"ScutiNativeAppStore :: Open App %d", appID);
        
        if (scutiNativeAppstorePlugin == nil)
            scutiNativeAppstorePlugin = [[ScutiNativeAppstore alloc] init];
        
        [scutiNativeAppstorePlugin openAppInStore: appID];
    }
}