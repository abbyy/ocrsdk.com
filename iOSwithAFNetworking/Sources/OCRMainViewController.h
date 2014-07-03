#import <UIKit/UIKit.h>
#import "OCRTextViewController.h"

@interface OCRMainViewController : UIViewController<UIImagePickerControllerDelegate, UINavigationControllerDelegate, UIAlertViewDelegate>

@property (weak, nonatomic) IBOutlet UIImageView *imageView;

@property (strong, nonatomic) UIAlertView *alertView;

@property (strong, nonatomic) OCRTextViewController *textViewController;

@end
