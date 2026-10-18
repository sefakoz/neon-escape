export default {
  async fetch(request, env) {
    const url = new URL(request.url);
    if (url.pathname === "/api/health") {
      return Response.json({ ok: true, project: "neon-escape", status: "prototype" }, {
        headers: { "Cache-Control": "no-store" }
      });
    }
    if (url.pathname.startsWith("/api/")) {
      return Response.json({ error: "API route not implemented" }, { status: 404 });
    }
    return env.ASSETS.fetch(request);
  }
};
