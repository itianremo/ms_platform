import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { AuthService } from '../services/authService';
import { User, Camera, Loader2, Save } from 'lucide-react';

const ProfilePage = () => {
    const { user, login } = useAuth(); // We might need to refresh user data context
    const navigate = useNavigate();
    const { showToast } = useToast();

    const [displayName, setDisplayName] = useState('');
    const [bio, setBio] = useState('');
    const [avatarUrl, setAvatarUrl] = useState('');
    const [loading, setLoading] = useState(false);

    // Initial load - if we had a way to get existing profile, we would load it here.
    // However, for "Profile Completion", it's usually empty.
    useEffect(() => {
        if (user && user.email) {
            // Pre-fill display name from email if empty?
            if (!displayName) setDisplayName(user.email.split('@')[0]);
        }
    }, [user]);

    const handleSave = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!displayName.trim()) {
            showToast("Display Name is required", "error");
            return;
        }

        setLoading(true);
        try {
            // Need to verify if AuthService.updateProfile exists or we create it
            // Assuming endpoint: PUT /api/Users/profile
            await AuthService.updateProfile({ // We'll add this method
                displayName,
                bio,
                avatarUrl,
                customDataJson: '{}'
            });

            showToast("Profile updated successfully!", "success");
            navigate('/'); // Go to Dashboard
        } catch (error) {
            console.error(error);
            showToast("Failed to update profile", "error");
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen bg-gray-50 flex flex-col items-center justify-center p-4">
            <div className="max-w-md w-full bg-white rounded-xl shadow-lg p-8 animate-fade-in">
                <div className="text-center mb-8">
                    <h1 className="text-2xl font-bold text-gray-900">Complete Your Profile</h1>
                    <p className="text-gray-500 mt-2">Tell us a bit more about yourself to get started.</p>
                </div>

                <form onSubmit={handleSave} className="space-y-6">

                    {/* Avatar Placeholder */}
                    <div className="flex justify-center">
                        <div className="relative group cursor-pointer">
                            <div className="w-24 h-24 rounded-full bg-gray-100 flex items-center justify-center border-2 border-dashed border-gray-300 group-hover:border-primary transition-colors">
                                {avatarUrl ? (
                                    <img src={avatarUrl} alt="Avatar" className="w-full h-full rounded-full object-cover" />
                                ) : (
                                    <User size={40} className="text-gray-400" />
                                )}
                            </div>
                            <div className="absolute inset-0 flex items-center justify-center bg-black/50 rounded-full opacity-0 group-hover:opacity-100 transition-opacity">
                                <Camera size={20} className="text-white" />
                            </div>
                            {/* In a real app, this would be a file input */}
                            <input
                                type="text" // Just a URL input for now
                                className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
                                placeholder="Enter Image URL"
                                onChange={(e) => {
                                    const url = prompt("Enter Avatar URL (Mock upload):");
                                    if (url) setAvatarUrl(url);
                                }}
                            />
                        </div>
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-gray-700">Display Name</label>
                        <input
                            type="text"
                            className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
                            value={displayName}
                            onChange={(e) => setDisplayName(e.target.value)}
                            placeholder="John Doe"
                            required
                        />
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-gray-700">Bio <span className="text-gray-400 font-normal">(Optional)</span></label>
                        <textarea
                            className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 resize-none"
                            value={bio}
                            onChange={(e) => setBio(e.target.value)}
                            placeholder="I'm a software engineer..."
                        />
                    </div>

                    <button
                        disabled={loading}
                        className="w-full h-11 inline-flex items-center justify-center rounded-md text-sm font-medium bg-primary text-primary-foreground hover:bg-primary/90 transition-colors disabled:opacity-50"
                    >
                        {loading ? <Loader2 className="animate-spin mr-2" /> : <Save className="mr-2" size={18} />}
                        {loading ? 'Saving Profile...' : 'Save & Continue'}
                    </button>

                    <button type="button" onClick={() => navigate('/')} className="w-full text-center text-sm text-gray-500 hover:text-gray-900 mt-2">
                        Skip for now
                    </button>
                </form>
            </div>
        </div>
    );
};

export default ProfilePage;
