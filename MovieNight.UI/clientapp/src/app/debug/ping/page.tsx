'use client';

import { useState } from 'react';

export default function PingGatewayPage() {
    const [path, setPath] = useState('healthz'); 
    const [method, setMethod] = useState<'GET'|'POST'|'PUT'|'PATCH'|'DELETE'>('GET');
    const [body, setBody] = useState('{"hello":"world"}');
    const [resp, setResp] = useState<string>('');

    async function send() {
        setResp('Sending...');
        try {
            const res = await fetch(`/api/gw/${path}`, {
                method,
                headers: (method === 'GET' || method === 'DELETE')
                    ? undefined
                    : { 'content-type': 'application/json' },
                body: (method === 'GET' || method === 'DELETE') ? undefined : body,
            });

            const text = await res.text();
            setResp(`HTTP ${res.status}\n\n${text}`);
        } catch (e: any) {
            setResp(`ERROR: ${e?.message ?? String(e)}`);
        }
    }

    return (
        <div className="p-6 max-w-2xl">
            <h1 className="text-2xl font-bold mb-4">Ping Gateway</h1>

            <label className="block text-sm mb-1">Path</label>
            <input
                className="w-full border rounded p-2 mb-3"
                value={path}
                onChange={(e) => setPath(e.target.value)}
                placeholder="healthz или debug/echo"
            />

            <label className="block text-sm mb-1">Method</label>
            <select
                className="w-full border rounded p-2 mb-3"
                value={method}
                onChange={(e) => setMethod(e.target.value as any)}
            >
                <option>GET</option>
                <option>POST</option>
                <option>PUT</option>
                <option>PATCH</option>
                <option>DELETE</option>
            </select>

            {!(method === 'GET' || method === 'DELETE') && (
                <>
                    <label className="block text-sm mb-1">Body (JSON или текст)</label>
                    <textarea
                        className="w-full border rounded p-2 font-mono text-sm h-32 mb-3"
                        value={body}
                        onChange={(e) => setBody(e.target.value)}
                    />
                </>
            )}

            <button
                className="rounded px-4 py-2 bg-black text-white"
                onClick={send}
            >
                Send
            </button>

            <h2 className="text-xl font-semibold mt-6 mb-2">Response</h2>
            <pre className="whitespace-pre-wrap border rounded p-3 bg-gray-50">
        {resp}
      </pre>
        </div>
    );
}
