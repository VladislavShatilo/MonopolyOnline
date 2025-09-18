using System;
using System.Collections;
public interface IFadeService
{
    bool IsFading { get; }
    void FadeIn(Action callback);
    void FadeOut(Action callback);
}

