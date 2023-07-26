using TarodevController;

/// <summary>
/// This is an example of how you can override the default behavior of the PlayerController.
/// </summary>
public class CustomExamplePlayerController : PlayerController
{
    // Here we're overriding how we handle crouch. Originally we used the y input axis to determine if we should crouch.
    protected override bool CrouchPressed => FrameInput.ExampleActionHeld;
}