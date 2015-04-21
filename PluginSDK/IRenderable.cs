using System;

namespace WorldWind.Renderable
{
	/// <summary>
	/// 
	/// </summary>
	interface IRenderable : IDisposable
	{
		void Initialize(DrawArgs drawArgs);
		void Update(DrawArgs drawArgs);
		void Render(DrawArgs drawArgs);
	}
}
