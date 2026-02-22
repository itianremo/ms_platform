import React, { useState, useEffect } from 'react';
import { Card, CardContent } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Check, X, ImageIcon, Video, Filter } from 'lucide-react';
import { useToast } from '../context/ToastContext';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/select';

// Mock Data structure for Media items since we don't have a MediaService yet
interface MediaItem {
    id: string;
    userId: string;
    userName: string;
    type: 'photo' | 'video';
    url: string;
    status: 'pending' | 'approved' | 'rejected';
    createdAt: string;
}

const MOCK_MEDIA: MediaItem[] = [
    {
        id: 'm1',
        userId: 'u1',
        userName: 'Alice Smith',
        type: 'photo',
        url: 'https://images.unsplash.com/photo-1544005313-94ddf0286df2?auto=format&fit=crop&q=80&w=400',
        status: 'pending',
        createdAt: new Date(Date.now() - 3600000).toISOString()
    },
    {
        id: 'm2',
        userId: 'u2',
        userName: 'John Doe',
        type: 'photo',
        url: 'https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?auto=format&fit=crop&q=80&w=400',
        status: 'pending',
        createdAt: new Date(Date.now() - 7200000).toISOString()
    },
    {
        id: 'm3',
        userId: 'u3',
        userName: 'Emma Wilson',
        type: 'video',
        url: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&q=80&w=400', // Mock video thumbnail
        status: 'approved',
        createdAt: new Date(Date.now() - 86400000).toISOString()
    }
];

const MediaPage: React.FC = () => {
    const { showToast } = useToast();
    const [mediaItems, setMediaItems] = useState<MediaItem[]>([]);
    const [loading, setLoading] = useState(true);
    const [filterStatus, setFilterStatus] = useState<'all' | 'pending' | 'approved' | 'rejected'>('pending');

    useEffect(() => {
        // Fetch media items
        const fetchMedia = async () => {
            setLoading(true);
            try {
                // Simulate API call
                await new Promise(resolve => setTimeout(resolve, 600));
                setMediaItems(MOCK_MEDIA);
            } catch (error) {
                console.error("Failed to load media", error);
                showToast("Failed to load media", "error");
            } finally {
                setLoading(false);
            }
        };

        fetchMedia();
    }, []);

    const handleReviewMedia = async (id: string, userId: string, isApproved: boolean) => {
        try {
            const statusText = isApproved ? "Approved" : "Rejected";

            // Optimistic update
            setMediaItems(prev => prev.map(item =>
                item.id === id ? { ...item, status: isApproved ? 'approved' : 'rejected' } : item
            ));

            showToast(`Media ${statusText}`, "success");

            // Admin sends notification to user about photo status
            const statusStr = isApproved ? "Approved" : "Rejected";
            await fetch('/notifications/api/Notifications', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    userId: userId,
                    title: `Media ${statusStr}`,
                    message: `Your recently uploaded media has been ${statusStr.toLowerCase()}.`,
                    link: '/profile/edit'
                })
            }).catch(() => {
                console.log("Mock notification sent to user for media review");
            });

        } catch (error) {
            showToast("Failed to update media status", "error");
        }
    };

    const filteredMedia = filterStatus === 'all'
        ? mediaItems
        : mediaItems.filter(item => item.status === filterStatus);

    return (
        <div className="space-y-6 animate-fade-in">
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                <div>
                    <h2 className="text-3xl font-bold tracking-tight">Media Review</h2>
                    <p className="text-muted-foreground">Manage and review user uploaded photos and videos.</p>
                </div>
                <div className="flex items-center gap-2">
                    <Filter className="h-4 w-4 text-muted-foreground mr-1" />
                    <Select value={filterStatus} onValueChange={(val: string) => setFilterStatus(val as any)}>
                        <SelectTrigger className="w-[150px]">
                            <SelectValue placeholder="Filter by status" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="all">All Media</SelectItem>
                            <SelectItem value="pending">Pending Review</SelectItem>
                            <SelectItem value="approved">Approved</SelectItem>
                            <SelectItem value="rejected">Rejected</SelectItem>
                        </SelectContent>
                    </Select>
                </div>
            </div>

            {loading ? (
                <div className="flex justify-center items-center h-64 text-muted-foreground">Loading media...</div>
            ) : filteredMedia.length === 0 ? (
                <div className="flex flex-col items-center justify-center p-12 bg-card border rounded-lg border-dashed">
                    <ImageIcon className="h-12 w-12 text-muted-foreground mb-4 opacity-50" />
                    <h3 className="text-lg font-medium">No media found</h3>
                    <p className="text-muted-foreground text-sm mt-1">There are no items matching the current filter.</p>
                </div>
            ) : (
                <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
                    {filteredMedia.map((item) => (
                        <Card key={item.id} className="overflow-hidden group flex flex-col">
                            <div className="relative aspect-square w-full">
                                <img
                                    src={item.url}
                                    alt="User upload"
                                    className="w-full h-full object-cover transition-transform duration-300 group-hover:scale-105"
                                />
                                <div className="absolute top-2 right-2 bg-black/60 backdrop-blur-md rounded-md p-1.5 text-white">
                                    {item.type === 'video' ? <Video size={16} /> : <ImageIcon size={16} />}
                                </div>
                                <div className={`absolute top-2 left-2 px-2 py-0.5 rounded text-[10px] font-bold uppercase tracking-wider text-white
                                    ${item.status === 'pending' ? 'bg-amber-500' : item.status === 'approved' ? 'bg-emerald-500' : 'bg-red-500'}`}>
                                    {item.status}
                                </div>
                            </div>
                            <CardContent className="p-4 space-y-3 bg-card border-t flex-1 flex flex-col justify-between relative z-10">
                                <div>
                                    <h4 className="font-medium text-sm truncate">{item.userName}</h4>
                                    <p className="text-xs text-muted-foreground">Uploaded {new Date(item.createdAt).toLocaleDateString()}</p>
                                </div>

                                {item.status === 'pending' && (
                                    <div className="flex gap-2 pt-2 mt-auto">
                                        <Button
                                            size="sm"
                                            className="flex-1 bg-emerald-600 hover:bg-emerald-700 text-white"
                                            onClick={() => handleReviewMedia(item.id, item.userId, true)}
                                        >
                                            <Check size={16} className="mr-1" /> Approve
                                        </Button>
                                        <Button
                                            size="sm"
                                            variant="outline"
                                            className="flex-1 text-red-600 border-red-200 hover:bg-red-50 hover:text-red-700"
                                            onClick={() => handleReviewMedia(item.id, item.userId, false)}
                                        >
                                            <X size={16} className="mr-1" /> Reject
                                        </Button>
                                    </div>
                                )}
                            </CardContent>
                        </Card>
                    ))}
                </div>
            )}
        </div>
    );
};

export default MediaPage;