#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>

// Root view controller of Unity screen
extern UIViewController *UnityGetGLViewController();

@interface ScutiNativeAppstore : NSObject <SKStoreProductViewControllerDelegate>

@end
