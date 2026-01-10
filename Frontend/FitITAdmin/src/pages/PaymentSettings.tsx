import React, { useState, useEffect } from 'react';



const PaymentSettings: React.FC = () => {
    const [gatewayName, setGatewayName] = useState('Mock');
    const [apiKey, setApiKey] = useState('');
    const [secretKey, setSecretKey] = useState('');
    const [webhookSecret, setWebhookSecret] = useState('');
    const [isEnabled, setIsEnabled] = useState(true);
    const [loading, setLoading] = useState(false);
    const [message, setMessage] = useState('');

    const appId = 'FitIT'; // Hardcoded for this Tenant Admin

    useEffect(() => {
        // Fetch existing config
        fetch(`/api/payments/config/${appId}`)
            .then(res => {
                if (res.ok) return res.json();
                return null;
            })
            .then(data => {
                if (data) {
                    setGatewayName(data.gatewayName);
                    setApiKey(data.apiKey);
                    setSecretKey(data.secretKey);
                    setWebhookSecret(data.webhookSecret);
                    setIsEnabled(data.isEnabled);
                }
            })
            .catch(err => console.error("Failed to load payment config", err));
    }, []);

    const handleSave = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setMessage('');

        try {
            const payload = {
                appId,
                gatewayName,
                apiKey,
                secretKey,
                webhookSecret,
                isEnabled
            };

            const response = await fetch('/api/payments/config', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (response.ok) {
                setMessage('Configuration saved successfully!');
            } else {
                setMessage('Failed to save configuration.');
            }
        } catch (error) {
            console.error(error);
            setMessage('Error saving configuration.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div style={{ padding: '2rem' }}>
            <h2>Payment Gateway Settings</h2>

            {message && <div style={{ marginBottom: '1rem', padding: '0.5rem', backgroundColor: '#e0f2fe', color: '#0369a1', borderRadius: '4px' }}>{message}</div>}

            <form onSubmit={handleSave} style={{ display: 'flex', flexDirection: 'column', gap: '1rem', maxWidth: '500px' }}>

                <div>
                    <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>Gateway Provider</label>
                    <select
                        value={gatewayName}
                        onChange={(e) => setGatewayName(e.target.value)}
                        style={{ width: '100%', padding: '0.5rem' }}
                    >
                        <option value="Mock">Mock Gateway (Test)</option>
                        <option value="Stripe">Stripe</option>
                        <option value="PayTabs">PayTabs</option>
                    </select>
                </div>

                <div>
                    <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>API Key</label>
                    <input
                        type="text"
                        value={apiKey}
                        onChange={(e) => setApiKey(e.target.value)}
                        style={{ width: '100%', padding: '0.5rem' }}
                    />
                </div>

                <div>
                    <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>Secret Key</label>
                    <input
                        type="password"
                        value={secretKey}
                        onChange={(e) => setSecretKey(e.target.value)}
                        style={{ width: '100%', padding: '0.5rem' }}
                    />
                </div>

                <div>
                    <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>Webhook Secret</label>
                    <input
                        type="password"
                        value={webhookSecret}
                        onChange={(e) => setWebhookSecret(e.target.value)}
                        style={{ width: '100%', padding: '0.5rem' }}
                    />
                </div>

                <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                    <input
                        type="checkbox"
                        checked={isEnabled}
                        onChange={(e) => setIsEnabled(e.target.checked)}
                        id="enabledCheck"
                    />
                    <label htmlFor="enabledCheck">Enable Payments</label>
                </div>

                <button
                    type="submit"
                    disabled={loading}
                    style={{
                        padding: '0.75rem',
                        backgroundColor: '#2563eb',
                        color: 'white',
                        border: 'none',
                        borderRadius: '4px',
                        cursor: loading ? 'not-allowed' : 'pointer'
                    }}
                >
                    {loading ? 'Saving...' : 'Save Configuration'}
                </button>
            </form>
        </div>
    );
};

export default PaymentSettings;
