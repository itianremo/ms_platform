
import React, { useState, useEffect } from 'react';
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "../../components/ui/dialog";
import { Button } from "../../components/ui/button";
import { Input } from "../../components/ui/input";
import { Label } from "../../components/ui/label";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "../../components/ui/select";
import { useToast } from "../../context/ToastContext";
import { UserService, UserProfile, UpdateProfileRequest } from '../../services/userService';

// Need to export this interface from UserService if not already
// Assuming UserService has updateProfile(request)

interface EditAppProfileDialogProps {
    isOpen: boolean;
    onClose: () => void;
    userId: string;
    appId: string;
    appName: string;
    description?: string;
    initialProfile?: UserProfile | null;
    onProfileUpdated: () => void;
}

export function EditAppProfileDialog({
    isOpen,
    onClose,
    userId,
    appId,
    appName,
    initialProfile,
    onProfileUpdated
}: EditAppProfileDialogProps) {
    const { showToast } = useToast();
    const [loading, setLoading] = useState(false);
    const [fetching, setFetching] = useState(false);

    // Form State
    const [displayName, setDisplayName] = useState('');
    const [bio, setBio] = useState('');
    const [gender, setGender] = useState('');
    const [dob, setDob] = useState('');
    const [customData, setCustomData] = useState('{}');

    useEffect(() => {
        if (isOpen) {
            if (initialProfile) {
                populateForm(initialProfile);
            } else {
                fetchProfile();
            }
        }
    }, [isOpen, initialProfile, userId, appId]);

    const fetchProfile = async () => {
        setFetching(true);
        try {
            const profile = await UserService.getProfile(userId, appId);
            if (profile) {
                populateForm(profile);
            } else {
                // Defaults if no profile exists yet (should be rare if seeded)
                setDisplayName(`User ${userId.substring(0, 5)}`);
                setCustomData('{}');
            }
        } catch (error) {
            console.error(error);
        } finally {
            setFetching(false);
        }
    };

    const populateForm = (p: UserProfile) => {
        setDisplayName(p.displayName || '');
        setBio(p.bio || '');
        setGender(p.gender || '');
        setDob(p.dateOfBirth ? p.dateOfBirth.split('T')[0] : '');
        setCustomData(p.customDataJson || '{}');
    };

    const handleSave = async () => {
        // Validate JSON
        try {
            JSON.parse(customData);
        } catch (e) {
            showToast("Invalid JSON in User Settings", "error");
            return;
        }

        setLoading(true);
        try {
            const request: UpdateProfileRequest = {
                userId,
                appId,
                displayName,
                bio,
                gender: gender || undefined,
                dateOfBirth: dob ? new Date(dob).toISOString() : undefined,
                customDataJson: customData,
                avatarUrl: undefined // Not editing avatar here
            };

            await UserService.updateProfile(request);

            showToast(`User profile for ${appName} has been updated.`, "success");
            onProfileUpdated();
            onClose();
        } catch (error) {
            showToast("Failed to update profile.", "error");
        } finally {
            setLoading(false);
        }
    };

    return (
        <Dialog open={isOpen} onOpenChange={onClose}>
            <DialogContent className="sm:max-w-2xl max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>Edit Profile: {appName}</DialogTitle>
                    <DialogDescription>
                        Update app-specific details for this user.
                    </DialogDescription>
                </DialogHeader>

                {fetching ? (
                    <div className="py-4 text-center">Loading profile...</div>
                ) : (
                    <div className="grid gap-4 py-4">
                        <div className="grid grid-cols-4 items-center gap-4">
                            <Label htmlFor="displayName" className="text-right">
                                Display Name
                            </Label>
                            <Input
                                id="displayName"
                                value={displayName}
                                onChange={(e: React.ChangeEvent<HTMLInputElement>) => setDisplayName(e.target.value)}
                                className="col-span-3"
                            />
                        </div>
                        <div className="grid grid-cols-4 items-center gap-4">
                            <Label htmlFor="bio" className="text-right">
                                Bio
                            </Label>
                            <Input
                                id="bio"
                                value={bio}
                                onChange={(e: React.ChangeEvent<HTMLInputElement>) => setBio(e.target.value)}
                                className="col-span-3"
                            />
                        </div>
                        <div className="grid grid-cols-4 items-center gap-4">
                            <Label htmlFor="gender" className="text-right">
                                Gender
                            </Label>
                            <Select onValueChange={setGender} value={gender}>
                                <SelectTrigger className="col-span-3">
                                    <SelectValue placeholder="Select gender" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="Male">Male</SelectItem>
                                    <SelectItem value="Female">Female</SelectItem>
                                    <SelectItem value="Non-Binary">Non-Binary</SelectItem>
                                    <SelectItem value="Prefer Not to Say">Prefer Not to Say</SelectItem>
                                </SelectContent>
                            </Select>
                        </div>
                        <div className="grid grid-cols-4 items-center gap-4">
                            <Label htmlFor="dob" className="text-right">
                                Birth Date
                            </Label>
                            <Input
                                id="dob"
                                type="date"
                                value={dob}
                                onChange={(e: React.ChangeEvent<HTMLInputElement>) => setDob(e.target.value)}
                                className="col-span-3"
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="customData">User Settings (JSON)</Label>
                            <textarea
                                id="customData"
                                className="flex min-h-[150px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 font-mono"
                                value={customData}
                                onChange={(e) => setCustomData(e.target.value)}
                                placeholder="{}"
                            />
                            <p className="text-xs text-muted-foreground">
                                Edit raw profile data (e.g. preferences, theme, sidebar settings).
                            </p>
                        </div>
                    </div>
                )}

                <DialogFooter>
                    <Button variant="outline" onClick={onClose} disabled={loading}>
                        Cancel
                    </Button>
                    <Button onClick={handleSave} disabled={loading || fetching}>
                        {loading ? "Saving..." : "Save Changes"}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}

