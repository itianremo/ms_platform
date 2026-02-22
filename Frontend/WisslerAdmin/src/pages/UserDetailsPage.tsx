import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, User, CreditCard, Settings, Shield, Image as ImageIcon, History, HeartHandshake, Save } from 'lucide-react';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '../components/ui/avatar';
import { Input } from '../components/ui/input';
import { Switch } from '../components/ui/switch';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/select';
import { Label } from '../components/ui/label';
import { cn } from '../lib/utils';
import { UserService, type UserDto, type UserProfile } from '../services/userService';
import { useToast } from '../context/ToastContext';
import { APP_ID } from '../config';

const TABS = [
    { id: 'profile', label: 'Profile & Media', icon: User },
    { id: 'subscriptions', label: 'Subscriptions', icon: CreditCard },
    { id: 'payments', label: 'Payment History', icon: History },
    { id: 'settings', label: 'Settings & Filters', icon: Settings },
    { id: 'roles', label: 'App Roles', icon: Shield },
    { id: 'interactions', label: 'Interactions', icon: HeartHandshake }
];

const UserDetailsPage: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { showToast } = useToast();
    const [user, setUser] = useState<UserDto | null>(null);
    const [profile, setProfile] = useState<UserProfile | null>(null);
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [activeTab, setActiveTab] = useState(TABS[0].id);

    // Mock Data States
    const [mediaItems, setMediaItems] = useState([
        { id: '1', url: 'https://i.pravatar.cc/300?img=1', status: 'Approved' },
        { id: '2', url: 'https://i.pravatar.cc/300?img=2', status: 'Pending' }
    ]);

    interface CustomData {
        settings: { pushNotifications: boolean; privacy: string };
        filters: { maxDistance: number; ageRange: [number, number] };
        photos?: Array<{ url: string; isApproved: boolean; isVerified: boolean; isActive: boolean; orderNo?: number }>;
        [key: string]: any;
    }

    const [customData, setCustomData] = useState<CustomData>({
        settings: { pushNotifications: true, privacy: 'public' },
        filters: { maxDistance: 50, ageRange: [18, 35] },
        profileFields: { height: 180, smoking: 'no', drinking: 'socially' }
    });

    useEffect(() => {
        if (!id) return;
        const fetchUser = async () => {
            try {
                const [allUsers, userProfile] = await Promise.all([
                    UserService.getAllUsers(),
                    UserService.getProfile(id, APP_ID)
                ]);
                const foundUser = allUsers.find(u => u.id === id);
                if (foundUser) {
                    setUser(foundUser);
                } else {
                    showToast("User not found", "error");
                    navigate('/users');
                }
                if (userProfile) {
                    setProfile(userProfile);
                    if (userProfile.customDataJson) {
                        try {
                            const parsed = JSON.parse(userProfile.customDataJson) || {};
                            setCustomData(prev => ({
                                ...prev,
                                ...parsed,
                                settings: { ...prev.settings, ...(parsed.settings || {}) },
                                filters: {
                                    maxDistance: parsed.filters?.maxDistance ?? prev.filters.maxDistance,
                                    ageRange: parsed.filters?.ageRange ?? prev.filters.ageRange
                                }
                            }));
                        } catch (e) {
                            console.error("Invalid customDataJson");
                        }
                    }
                }
            } catch (err) {
                console.error(err);
                showToast("Failed to load user info", "error");
            } finally {
                setLoading(false);
            }
        };
        fetchUser();
    }, [id, navigate, showToast]);

    const handleSaveProfile = async () => {
        if (!profile) return;
        setSaving(true);
        try {
            const updatedProfile = {
                ...profile,
                customDataJson: JSON.stringify(customData)
            };
            await UserService.updateProfile(updatedProfile);
            setProfile(updatedProfile);
            showToast("Profile settings saved successfully", "success");
        } catch (error) {
            showToast("Failed to save settings", "error");
        } finally {
            setSaving(false);
        }
    };

    const handleSetAsAvatar = async (photoUrl: string) => {
        if (!profile || !customData.photos) return;

        let newPhotos = [...customData.photos];

        // Remove the target photo and place it at the front
        const targetPhotoIndex = newPhotos.findIndex(p => p.url === photoUrl);
        if (targetPhotoIndex > -1) {
            const target = newPhotos.splice(targetPhotoIndex, 1)[0];
            newPhotos.unshift(target);

            // Re-assign orderNo
            newPhotos = newPhotos.map((p, index) => ({ ...p, orderNo: index + 1 }));

            setCustomData({ ...customData, photos: newPhotos });

            try {
                // Update profile avatarUrl explicitly as well as saving customData
                const updatedProfile = {
                    ...profile,
                    avatarUrl: photoUrl,
                    customDataJson: JSON.stringify({ ...customData, photos: newPhotos })
                };
                await UserService.updateProfile(updatedProfile);
                setProfile(updatedProfile);
                // Try and update the user state too if they are looking at header
                setUser(prev => prev ? { ...prev, photoUrl: photoUrl } : null);
                showToast("Avatar updated successfully", "success");
            } catch (error) {
                showToast("Failed to update avatar", "error");
            }
        }
    };

    if (loading) return <div className="p-8 flex items-center justify-center min-h-[50vh]">Loading user profile...</div>;
    if (!user) return null;

    return (
        <div className="p-4 md:p-8 pt-6 space-y-6 animate-fade-in max-w-7xl mx-auto">
            {/* Header */}
            <div className="flex items-center space-x-4 border-b pb-4">
                <Button variant="ghost" size="icon" onClick={() => navigate('/users')}>
                    <ArrowLeft className="h-5 w-5" />
                </Button>
                <Avatar className="h-16 w-16">
                    <AvatarImage src={user.photoUrl || profile?.avatarUrl} />
                    <AvatarFallback className="text-xl">
                        {(user.displayName || user.firstName || 'U').substring(0, 2).toUpperCase()}
                    </AvatarFallback>
                </Avatar>
                <div className="flex-1">
                    <h2 className="text-2xl font-bold tracking-tight">{user.displayName || `${user.firstName} ${user.lastName}`}</h2>
                    <p className="text-muted-foreground">{user.email} • {user.isActive ? 'Active' : 'Inactive'}</p>
                </div>
                <div className="flex gap-2">
                    <Button variant={user.isActive ? "destructive" : "default"}>
                        {user.isActive ? "Deactivate User" : "Activate User"}
                    </Button>
                </div>
            </div>

            <div className="flex flex-col md:flex-row gap-6">
                {/* Vertical Tabs Sidemenu */}
                <Card className="w-full md:w-64 shrink-0 h-fit">
                    <CardContent className="p-2 space-y-1">
                        {TABS.map(tab => {
                            const Icon = tab.icon;
                            return (
                                <button
                                    key={tab.id}
                                    onClick={() => setActiveTab(tab.id)}
                                    className={cn(
                                        "flex items-center w-full px-4 py-3 text-sm font-medium rounded-md transition-colors",
                                        activeTab === tab.id
                                            ? "bg-primary text-primary-foreground"
                                            : "text-muted-foreground hover:bg-secondary hover:text-foreground"
                                    )}
                                >
                                    <Icon className="mr-3 h-5 w-5" />
                                    {tab.label}
                                </button>
                            );
                        })}
                    </CardContent>
                </Card>

                {/* Main Content Area */}
                <Card className="flex-1 min-h-[500px]">
                    <CardHeader className="flex flex-row items-center justify-between">
                        <CardTitle>{TABS.find(t => t.id === activeTab)?.label}</CardTitle>
                        {activeTab === 'settings' && (
                            <Button onClick={handleSaveProfile} disabled={saving} size="sm">
                                <Save className="h-4 w-4 mr-2" />
                                {saving ? 'Saving...' : 'Save Settings'}
                            </Button>
                        )}
                    </CardHeader>
                    <CardContent>
                        {activeTab === 'profile' && (
                            <div className="space-y-8">
                                <div className="grid grid-cols-2 gap-4">
                                    <div className="space-y-1"><Label>First Name</Label><Input value={user.firstName} readOnly /></div>
                                    <div className="space-y-1"><Label>Last Name</Label><Input value={user.lastName} readOnly /></div>
                                    <div className="space-y-1"><Label>Email</Label><Input value={user.email} readOnly /></div>
                                    <div className="space-y-1"><Label>Phone</Label><Input value={user.phone || ''} readOnly /></div>
                                </div>

                                <div>
                                    <div className="flex items-center justify-between mb-4">
                                        <h3 className="text-lg font-semibold">Additional Profile Fields</h3>
                                        <Button size="sm" onClick={handleSaveProfile} disabled={saving} variant="outline">
                                            {saving ? 'Saving...' : 'Save Profile Fields'}
                                        </Button>
                                    </div>
                                    <div className="grid grid-cols-2 md:grid-cols-3 gap-4 border p-4 rounded-md">
                                        <div className="space-y-1"><Label>Height (cm)</Label><Input type="number" value={customData.height || ''} onChange={(e) => setCustomData({ ...customData, height: e.target.value })} /></div>
                                        <div className="space-y-1"><Label>Weight (kg)</Label><Input type="number" value={customData.weight || ''} onChange={(e) => setCustomData({ ...customData, weight: e.target.value })} /></div>
                                        <div className="space-y-1"><Label>Job</Label><Input value={customData.job || ''} onChange={(e) => setCustomData({ ...customData, job: e.target.value })} /></div>
                                        <div className="space-y-1"><Label>Education</Label><Input value={customData.education || ''} onChange={(e) => setCustomData({ ...customData, education: e.target.value })} /></div>
                                        <div className="space-y-1"><Label>Smoking</Label><Input value={customData.smoking || ''} onChange={(e) => setCustomData({ ...customData, smoking: e.target.value })} /></div>
                                        <div className="space-y-1"><Label>Drinking</Label><Input value={customData.drinking || ''} onChange={(e) => setCustomData({ ...customData, drinking: e.target.value })} /></div>
                                        <div className="space-y-1"><Label>City</Label><Input value={customData.cityId || ''} onChange={(e) => setCustomData({ ...customData, cityId: e.target.value })} /></div>
                                        <div className="space-y-1"><Label>Country</Label><Input value={customData.countryId || ''} onChange={(e) => setCustomData({ ...customData, countryId: e.target.value })} /></div>
                                    </div>
                                </div>

                                <div className="pt-4 border-t">
                                    <div className="flex items-center justify-between mb-4">
                                        <h3 className="text-lg font-semibold">Media Gallery</h3>
                                        <Button size="sm" onClick={handleSaveProfile} disabled={saving}>
                                            {saving ? 'Saving...' : 'Save Media Changes'}
                                        </Button>
                                    </div>
                                    {!customData.photos || customData.photos.length === 0 ? (
                                        <p className="text-sm text-muted-foreground">No photos found for this user.</p>
                                    ) : (
                                        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
                                            {customData.photos.sort((a, b) => (a.orderNo || 99) - (b.orderNo || 99)).map((media, idx) => (
                                                <div key={idx} className={cn("border rounded-md p-2 space-y-2 relative group flex flex-col items-center", media.orderNo === 1 || idx === 0 ? "border-primary ring-2 ring-primary ring-offset-2" : "")}>
                                                    {(media.orderNo === 1 || idx === 0) && (
                                                        <div className="absolute -top-3 -right-3 bg-primary text-white text-xs px-2 py-1 rounded-full font-bold shadow-md">Avatar</div>
                                                    )}
                                                    <img src={media.url} alt="User media" className="w-full h-32 object-cover rounded-md" />
                                                    <div className="w-full space-y-2">
                                                        <Select value={media.isApproved ? "Approved" : "Pending"} onValueChange={(val) => {
                                                            const newPhotos = [...(customData.photos || [])];
                                                            const pIdx = newPhotos.findIndex(p => p.url === media.url);
                                                            if (pIdx > -1) newPhotos[pIdx].isApproved = val === "Approved";
                                                            setCustomData({ ...customData, photos: newPhotos });
                                                        }}>
                                                            <SelectTrigger className="w-full text-xs h-8">
                                                                <SelectValue />
                                                            </SelectTrigger>
                                                            <SelectContent>
                                                                <SelectItem value="Pending">Pending</SelectItem>
                                                                <SelectItem value="Approved">Approved</SelectItem>
                                                                <SelectItem value="Rejected">Rejected</SelectItem>
                                                            </SelectContent>
                                                        </Select>
                                                        <Button
                                                            variant="secondary"
                                                            size="sm"
                                                            className="w-full text-xs"
                                                            onClick={() => handleSetAsAvatar(media.url)}
                                                            disabled={media.orderNo === 1 || idx === 0}
                                                        >
                                                            Set as Avatar
                                                        </Button>
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    )}
                                </div>
                            </div>
                        )}
                        {activeTab === 'subscriptions' && (
                            <div className="space-y-6">
                                <div className="border rounded-lg p-4 bg-muted/20">
                                    <h3 className="font-semibold mb-2">Active Subscription</h3>
                                    <div className="flex justify-between items-center text-sm">
                                        <div>
                                            <span className="font-medium text-primary">Gold Plan</span> • Starts: 2026-01-01 • Ends: 2026-12-31
                                        </div>
                                        <div className="px-2 py-1 bg-green-100 text-green-800 rounded-full text-xs font-semibold">ACTIVE</div>
                                    </div>
                                </div>
                                <div>
                                    <h3 className="font-semibold text-lg mb-4">Add New Subscription</h3>
                                    <div className="flex gap-4">
                                        <div className="space-y-1 flex-1">
                                            <Label>Start Date</Label>
                                            <Input type="date" min="2027-01-01" />
                                        </div>
                                        <div className="space-y-1 flex-1">
                                            <Label>End Date</Label>
                                            <Input type="date" />
                                        </div>
                                    </div>
                                    <Button className="mt-4">Assign Package</Button>
                                </div>
                            </div>
                        )}
                        {activeTab === 'payments' && (
                            <div className="space-y-6">
                                <p className="text-muted-foreground">No recent transactions found.</p>
                            </div>
                        )}
                        {activeTab === 'settings' && (
                            <div className="space-y-8 max-w-2xl">
                                <div>
                                    <h3 className="text-lg font-semibold mb-4 border-b pb-2">Settings</h3>
                                    <div className="space-y-4">
                                        <div className="flex items-center justify-between">
                                            <Label>Push Notifications</Label>
                                            <Switch
                                                checked={customData.settings.pushNotifications}
                                                onCheckedChange={(c) => setCustomData({ ...customData, settings: { ...customData.settings, pushNotifications: c } })}
                                            />
                                        </div>
                                        <div className="flex items-center justify-between">
                                            <Label>Privacy Profile</Label>
                                            <Select value={customData.settings.privacy} onValueChange={(v) => setCustomData({ ...customData, settings: { ...customData.settings, privacy: v } })}>
                                                <SelectTrigger className="w-1/2"><SelectValue /></SelectTrigger>
                                                <SelectContent>
                                                    <SelectItem value="public">Public</SelectItem>
                                                    <SelectItem value="private">Private</SelectItem>
                                                    <SelectItem value="friends">Friends Only</SelectItem>
                                                </SelectContent>
                                            </Select>
                                        </div>
                                    </div>
                                </div>
                                <div>
                                    <h3 className="text-lg font-semibold mb-4 border-b pb-2">Filters</h3>
                                    <div className="space-y-6">
                                        <div className="space-y-3">
                                            <div className="flex justify-between">
                                                <Label>Max Distance ({customData.filters.maxDistance} km)</Label>
                                            </div>
                                            <input
                                                type="range"
                                                className="w-full accent-primary"
                                                value={customData.filters.maxDistance}
                                                max={100}
                                                min={1}
                                                step={1}
                                                onChange={(e) => setCustomData({ ...customData, filters: { ...customData.filters, maxDistance: parseInt(e.target.value) } })}
                                            />
                                        </div>
                                        <div className="space-y-3">
                                            <div className="flex justify-between">
                                                <Label>Min Age ({customData.filters.ageRange[0]})</Label>
                                            </div>
                                            <input
                                                type="range"
                                                className="w-full accent-primary"
                                                value={customData.filters.ageRange[0]}
                                                max={100} min={18} step={1}
                                                onChange={(e) => setCustomData({ ...customData, filters: { ...customData.filters, ageRange: [parseInt(e.target.value), Math.max(parseInt(e.target.value), customData.filters.ageRange[1])] } })}
                                            />
                                        </div>
                                        <div className="space-y-3">
                                            <div className="flex justify-between">
                                                <Label>Max Age ({customData.filters.ageRange[1]})</Label>
                                            </div>
                                            <input
                                                type="range"
                                                className="w-full accent-primary"
                                                value={customData.filters.ageRange[1]}
                                                max={100} min={18} step={1}
                                                onChange={(e) => setCustomData({ ...customData, filters: { ...customData.filters, ageRange: [Math.min(parseInt(e.target.value), customData.filters.ageRange[0]), parseInt(e.target.value)] } })}
                                            />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        )}
                        {activeTab === 'roles' && (
                            <div className="space-y-4">
                                <Label>Assign App Roles</Label>
                                <Select defaultValue="ManageUsers">
                                    <SelectTrigger className="w-[300px]">
                                        <SelectValue placeholder="Select a role to assign" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        <SelectItem value="ManageUsers">Manage Users</SelectItem>
                                        <SelectItem value="ManageMedia">Manage Media</SelectItem>
                                        <SelectItem value="ManageConfigs">Manage Configs</SelectItem>
                                        <SelectItem value="WisslerAdmin">Wissler Admin</SelectItem>
                                    </SelectContent>
                                </Select>
                            </div>
                        )}
                        {activeTab === 'interactions' && (
                            <div className="space-y-8">
                                <div>
                                    <h3 className="text-lg font-semibold mb-4 text-emerald-600 flex items-center gap-2">
                                        <HeartHandshake className="h-5 w-5" /> Matched
                                    </h3>
                                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                                        {[1, 2].map(i => (
                                            <div key={i} className="flex items-center gap-3 p-3 border rounded-lg bg-emerald-50/50">
                                                <Avatar><AvatarImage src={`https://i.pravatar.cc/300?img=${i + 10}`} /></Avatar>
                                                <div className="flex flex-col">
                                                    <span className="font-semibold text-sm">Demo User {i}</span>
                                                    <span className="text-xs text-muted-foreground">Matched 2 days ago</span>
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                                <div>
                                    <h3 className="text-lg font-semibold mb-4 text-blue-600">Liked</h3>
                                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                                        {[3, 4, 5].map(i => (
                                            <div key={i} className="flex items-center gap-3 p-3 border rounded-lg">
                                                <Avatar><AvatarImage src={`https://i.pravatar.cc/300?img=${i + 20}`} /></Avatar>
                                                <div className="flex flex-col">
                                                    <span className="font-semibold text-sm">Demo User {i}</span>
                                                    <span className="text-xs text-muted-foreground">Liked 1 week ago</span>
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                                <div className="opacity-75">
                                    <h3 className="text-lg font-semibold mb-4 text-red-600">Disliked / Passed</h3>
                                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                                        {[6, 7].map(i => (
                                            <div key={i} className="flex items-center gap-3 p-3 border rounded-lg bg-muted/20">
                                                <Avatar><AvatarImage src={`https://i.pravatar.cc/300?img=${i + 30}`} /></Avatar>
                                                <div className="flex flex-col">
                                                    <span className="font-semibold text-sm">Demo User {i}</span>
                                                    <span className="text-xs text-muted-foreground">Passed yesterday</span>
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            </div>
                        )}
                    </CardContent>
                </Card>
            </div>
        </div>
    );
};

export default UserDetailsPage;
